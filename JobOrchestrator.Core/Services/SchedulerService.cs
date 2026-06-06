using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JobOrchestrator.Core.Models;
using JobOrchestrator.Core.Data;
using JobOrchestrator.Core.Exceptions;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Service for orchestrating job scheduling and execution.
/// This is an internal service managed by the Job Orchestrator framework.
/// </summary>
public interface ISchedulerService
{
    /// <summary>Starts the scheduler background process.</summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stops the scheduler.</summary>
    Task StopAsync();

    /// <summary>Executes a specific job immediately.</summary>
    Task ExecuteJobAsync(Guid jobId, Func<Job, Task>? executionHandler = null);
}

/// <summary>
/// Implementation of ISchedulerService.
/// This is an internal implementation detail.
/// </summary>
internal class SchedulerService : ISchedulerService
{
    private readonly IDbContextFactory<SchedulerDbContext> _contextFactory;
    private readonly ICronValidationService _cronValidator;
    private readonly SchedulerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SchedulerService> _logger;
    private Timer? _timer;
    private bool _isRunning;
    private readonly SemaphoreSlim _executionSemaphore;

    /// <summary>
    /// Creates a new instance of the scheduler service.
    /// </summary>
    public SchedulerService(
        IDbContextFactory<SchedulerDbContext> contextFactory,
        ICronValidationService cronValidator,
        SchedulerOptions options,
        IServiceProvider serviceProvider,
        ILogger<SchedulerService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _cronValidator = cronValidator ?? throw new ArgumentNullException(nameof(cronValidator));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _executionSemaphore = new SemaphoreSlim(options.MaxParallelJobs);
    }

    /// <summary>
    /// Starts the scheduler background process which polls for due jobs.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Scheduler is already running");
            return;
        }

        _isRunning = true;
        _logger.LogInformation("Scheduler starting with polling interval: {PollingInterval}s, max parallel jobs: {MaxJobs}",
            _options.PollingIntervalSeconds, _options.MaxParallelJobs);

        _timer = new Timer(
            CheckAndExecuteJobs,
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the scheduler and cancels any pending job polling.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Scheduler is not running");
            return;
        }

        _isRunning = false;
        _logger.LogInformation("Scheduler stopping");

        if (_timer != null)
        {
            await _timer.DisposeAsync();
            _timer = null;
        }
    }

    /// <summary>
    /// Executes a specific job immediately, bypassing the schedule.
    /// </summary>
    public async Task ExecuteJobAsync(Guid jobId, Func<Job, Task>? executionHandler = null)
    {
        _logger.LogInformation("Executing job: {JobId}", jobId);

        await _executionSemaphore.WaitAsync();
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var job = await context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
            
            if (job == null)
            {
                _logger.LogWarning("Job not found for execution: {JobId}", jobId);
                throw new JobNotFoundException(jobId);
            }

            if (!job.IsActive)
            {
                _logger.LogInformation("Job is inactive, skipping execution: {JobId}", jobId);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = executionHandler != null
                ? (Func<Job, CancellationToken, Task>)((j, _) => executionHandler(j))
                : BuildHandlerDelegate(ResolveHandler(scope.ServiceProvider, job.HandlerType));

            await ExecuteJobInternalAsync(job, handler, 0, scope.ServiceProvider);
        }
        finally
        {
            _executionSemaphore.Release();
        }
    }

    private async void CheckAndExecuteJobs(object? state)
    {
        if (!_isRunning)
            return;

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var now = DateTime.UtcNow;
            var jobsToExecute = await context.Jobs
                .Where(j => j.IsActive && j.NextExecutionTime <= now)
                .ToListAsync();

            if (jobsToExecute.Count > 0)
            {
                _logger.LogInformation("Found {JobCount} jobs due for execution", jobsToExecute.Count);
            }

            var tasks = jobsToExecute.Select(async job =>
            {
                await _executionSemaphore.WaitAsync();
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var handler = ResolveHandler(scope.ServiceProvider, job.HandlerType);
                    await ExecuteJobInternalAsync(job, handler, 0, scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing job {JobId}", job.Id);
                }
                finally
                {
                    _executionSemaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in job check cycle");
        }
    }

    private async Task ExecuteJobInternalAsync(Job job, Func<Job, CancellationToken, Task> handler, int retryCount, IServiceProvider scopedProvider)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            StartTime = DateTime.UtcNow,
            Status = JobExecutionStatus.Running,
            ExecutionAttempt = retryCount + 1
        };

        context.JobExecutions.Add(execution);
        await context.SaveChangesAsync();

        _logger.LogInformation("Executing job {JobId} (attempt {Attempt})", job.Id, execution.ExecutionAttempt);

        try
        {
            await handler(job, CancellationToken.None);

            execution.Status = JobExecutionStatus.Completed;
            execution.EndTime = DateTime.UtcNow;
            job.LastExecutedAt = DateTime.UtcNow;
            job.NextExecutionTime = _cronValidator.GetNextExecutionTime(job.CronExpression, DateTime.UtcNow);

            _logger.LogInformation("Job {JobId} completed successfully in {Duration}ms",
                job.Id, (execution.EndTime.Value - execution.StartTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            execution.ErrorMessage = ex.Message;
            execution.EndTime = DateTime.UtcNow;

            _logger.LogWarning(ex, "Job {JobId} execution failed (attempt {Attempt}): {Error}",
                job.Id, execution.ExecutionAttempt, ex.Message);

            var shouldRetry = retryCount < job.MaxRetries;
            if (shouldRetry)
            {
                execution.Status = JobExecutionStatus.Retrying;
                var retryDelay = (int)(job.RetryIntervalSeconds * Math.Pow((double)job.BackoffMultiplier, retryCount));
                job.NextExecutionTime = DateTime.UtcNow.AddSeconds(retryDelay);

                _logger.LogInformation("Job {JobId} will be retried in {RetryDelay}s (attempt {NextAttempt} of {MaxRetries})",
                    job.Id, retryDelay, execution.ExecutionAttempt + 1, job.MaxRetries);

                _ = Task.Delay(TimeSpan.FromSeconds(retryDelay)).ContinueWith(async _ =>
                {
                    using var retryScope = _serviceProvider.CreateScope();
                    var retryHandler = ResolveHandler(retryScope.ServiceProvider, job.HandlerType);
                    await ExecuteJobInternalAsync(job, retryHandler, retryCount + 1, retryScope.ServiceProvider);
                });
            }
            else
            {
                execution.Status = JobExecutionStatus.Failed;
                job.NextExecutionTime = _cronValidator.GetNextExecutionTime(job.CronExpression, DateTime.UtcNow);
                _logger.LogError("Job {JobId} failed permanently after {Attempts} attempts", job.Id, execution.ExecutionAttempt);
            }
        }

        context.JobExecutions.Update(execution);
        context.Jobs.Update(job);
        await context.SaveChangesAsync();
    }

    private Func<Job, CancellationToken, Task> ResolveHandler(IServiceProvider sp, string jobType)
    {
        try
        {
            var handlers = (IEnumerable<IJobHandler>?)sp.GetService(typeof(IEnumerable<IJobHandler>)) ?? Enumerable.Empty<IJobHandler>();
            var handler = handlers.FirstOrDefault(h =>
                string.Equals(h.GetType().FullName, jobType, StringComparison.OrdinalIgnoreCase));

            if (handler != null)
            {
                _logger.LogDebug("Handler found for job type: {JobType}", jobType);
                return (job, ct) => handler.ExecuteAsync(job, ct);
            }

            _logger.LogWarning("No handler registered for job type: {HandlerType}", jobType);
            return (job, _) =>
            {
                _logger.LogWarning("Skipping job '{JobName}' (ID: {JobId}) - no handler for type '{HandlerType}'",
                    job.Name, job.Id, job.HandlerType);
                return Task.CompletedTask;
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving handler for job type: {JobType}", jobType);
            throw;
        }
    }

    private static Func<Job, CancellationToken, Task> BuildHandlerDelegate(Func<Job, CancellationToken, Task> fn) => fn;
}
