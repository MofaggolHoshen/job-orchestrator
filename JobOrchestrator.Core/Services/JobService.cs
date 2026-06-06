using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobOrchestrator.Core.Models;
using JobOrchestrator.Core.DTOs;
using JobOrchestrator.Core.Data;
using JobOrchestrator.Core.Repositories;
using JobOrchestrator.Core.Mapping;
using JobOrchestrator.Core.Exceptions;
using JobOrchestrator.Core.Constants;
using JobOrchestrator.Core.Extensions;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Service for managing job CRUD operations.
/// 
/// Provides methods to create, retrieve, update, and delete scheduled jobs.
/// All methods throw Job Orchestrator exceptions on error - always wrap calls in try-catch blocks.
/// 
/// Exception Handling Guide:
/// - InvalidCronExpressionException: Thrown when cron expression is invalid
/// - JobNotFoundException: Thrown when job cannot be found
/// - JobOrchestratorException: Thrown for other unexpected errors
/// 
/// Example:
/// <code>
/// try
/// {
///     var job = await jobService.CreateJobAsync(new CreateJobRequest
///     {
///         Name = "Daily Backup",
///         HandlerType = "BackupDatabase",
///         CronExpression = "0 2 * * *"
///     });
/// }
/// catch (InvalidCronExpressionException ex)
/// {
///     _logger.LogWarning("Invalid cron: {Cron}", ex.CronExpression);
/// }
/// catch (JobOrchestratorException ex)
/// {
///     _logger.LogError(ex, "Failed to create job");
/// }
/// </code>
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Creates a new job with the specified configuration.
    /// </summary>
    /// <param name="request">The job creation request with name, type, and schedule.</param>
    /// <returns>The created job DTO with assigned ID.</returns>
    /// <exception cref="InvalidCronExpressionException">Thrown if the cron expression is invalid.</exception>
    /// <exception cref="JobOrchestratorException">Thrown if job creation fails for other reasons.</exception>
    Task<JobDto> CreateJobAsync(CreateJobRequest request);

    /// <summary>
    /// Retrieves a job by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the job.</param>
    /// <returns>The job DTO if found; null if not found.</returns>
    Task<JobDto?> GetJobAsync(Guid id);

    /// <summary>
    /// Retrieves all jobs in the system.
    /// </summary>
    /// <returns>A list of all jobs (active and inactive).</returns>
    Task<List<JobDto>> GetAllJobsAsync();

    /// <summary>
    /// Retrieves only active jobs.
    /// </summary>
    /// <returns>A list of jobs where IsActive = true.</returns>
    Task<List<JobDto>> GetActiveJobsAsync();

    /// <summary>
    /// Updates an existing job's configuration.
    /// </summary>
    /// <param name="id">The ID of the job to update.</param>
    /// <param name="request">The update request with new configuration values.</param>
    /// <returns>The updated job DTO.</returns>
    /// <exception cref="JobNotFoundException">Thrown if the job does not exist.</exception>
    /// <exception cref="InvalidCronExpressionException">Thrown if the new cron expression is invalid.</exception>
    /// <exception cref="JobOrchestratorException">Thrown if update fails for other reasons.</exception>
    Task<JobDto> UpdateJobAsync(Guid id, UpdateJobRequest request);

    /// <summary>
    /// Creates a rescheduled version of the job.
    /// The original job is preserved, and a new job is created with the new schedule.
    /// </summary>
    /// <param name="id">The ID of the job to reschedule.</param>
    /// <param name="request">The reschedule request with new cron expression and other options.</param>
    /// <returns>The new job DTO with the updated schedule.</returns>
    /// <exception cref="JobNotFoundException">Thrown if the job does not exist.</exception>
    /// <exception cref="InvalidCronExpressionException">Thrown if the new cron expression is invalid.</exception>
    Task<JobDto> RescheduleJobAsync(Guid id, RescheduleJobRequest request);

    /// <summary>
    /// Deletes a job from the system permanently.
    /// </summary>
    /// <param name="id">The ID of the job to delete.</param>
    /// <exception cref="JobNotFoundException">Thrown if the job does not exist.</exception>
    /// <exception cref="JobOrchestratorException">Thrown if deletion fails.</exception>
    Task DeleteJobAsync(Guid id);

    /// <summary>
    /// Toggles the active status of a job (enabled/disabled).
    /// </summary>
    /// <param name="id">The ID of the job whose status should be toggled.</param>
    /// <returns>The new active status (true if now active, false if now inactive).</returns>
    /// <exception cref="JobNotFoundException">Thrown if the job does not exist.</exception>
    Task<bool> ToggleJobStatusAsync(Guid id);
}

/// <summary>
/// Implementation of IJobService.
/// This is an internal implementation detail - use IJobService interface for dependency injection.
/// </summary>
internal class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly ICronValidationService _cronValidator;
    private readonly IMappingService _mappingService;
    private readonly IJobHandlerRegistry _handlerRegistry;
    private readonly ILogger<JobService> _logger;

    /// <summary>
    /// Creates a new instance of the job service.
    /// </summary>
    public JobService(
        IJobRepository repository,
        ICronValidationService cronValidator,
        IMappingService mappingService,
        IJobHandlerRegistry handlerRegistry,
        ILogger<JobService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cronValidator = cronValidator ?? throw new ArgumentNullException(nameof(cronValidator));
        _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
        _handlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JobDto> CreateJobAsync(CreateJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        // Validate input
        request.Name.ValidateNotNullOrEmpty(nameof(request.Name), ErrorMessages.InvalidJobName);
        request.HandlerType.ValidateNotNullOrEmpty(nameof(request.HandlerType), ErrorMessages.InvalidJobType);
        request.CronExpression.ValidateNotNullOrEmpty(nameof(request.CronExpression), ErrorMessages.InvalidCronExpression);

        _logger.LogInformation("Creating job: {JobName} of type {HandlerType}", request.Name, request.HandlerType);

        // Validate cron expression
        var (isValid, error) = _cronValidator.ValidateCronExpression(request.CronExpression);
        if (!isValid)
        {
            _logger.LogWarning("Invalid cron expression provided: {CronExpression}", request.CronExpression);
            throw new InvalidCronExpressionException(request.CronExpression, error);
        }

        // Calculate next execution time
        var nextExecutionTime = _cronValidator.GetNextExecutionTime(request.CronExpression);
        if (!nextExecutionTime.HasValue)
        {
            _logger.LogError("Failed to calculate next execution time for cron: {CronExpression}", request.CronExpression);
            throw new InvalidCronExpressionException(request.CronExpression, ErrorMessages.CannotCalculateNextExecution);
        }

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HandlerType = request.HandlerType,
            HandlerName = GetHandlerName(request.HandlerType) ?? request.HandlerType,
            CronExpression = request.CronExpression,
            IsActive = true,
            NextExecutionTime = nextExecutionTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Configure retry policy if provided
        if (request.MaxRetries.HasValue)
        {
            request.MaxRetries.Value.ValidateNonNegative(nameof(request.MaxRetries), ErrorMessages.InvalidMaxRetries);
            var retryInterval = request.RetryIntervalSeconds ?? 60;
            retryInterval.ValidatePositive(nameof(retryInterval), ErrorMessages.InvalidRetryInterval);
            var backoffMultiplier = request.BackoffMultiplier ?? 1.5m;
            backoffMultiplier.ValidatePositive(nameof(backoffMultiplier), ErrorMessages.InvalidBackoffMultiplier);

            job.MaxRetries = request.MaxRetries.Value;
            job.RetryIntervalSeconds = retryInterval;
            job.BackoffMultiplier = backoffMultiplier;

            _logger.LogInformation("Retry policy configured for job {JobId}: MaxRetries={MaxRetries}, Interval={Interval}s, Backoff={Backoff}",
                job.Id, job.MaxRetries, job.RetryIntervalSeconds, job.BackoffMultiplier);
        }

        await _repository.AddAsync(job);
        _logger.LogInformation("Job created successfully: {JobId}", job.Id);

        return _mappingService.MapToJobDto(job);
    }

    public async Task<JobDto?> GetJobAsync(Guid id)
    {
        _logger.LogInformation("Retrieving job: {JobId}", id);
        var job = await _repository.GetByIdAsync(id);
        return job != null ? _mappingService.MapToJobDto(job) : null;
    }

    public async Task<List<JobDto>> GetAllJobsAsync()
    {
        _logger.LogInformation("Retrieving all jobs");
        var jobs = await _repository.GetAllAsync();
        return _mappingService.MapToJobDtos(jobs).ToList();
    }

    public async Task<List<JobDto>> GetActiveJobsAsync()
    {
        _logger.LogInformation("Retrieving active jobs");
        var jobs = await _repository.GetActiveAsync();
        return _mappingService.MapToJobDtos(jobs).ToList();
    }

    public async Task<JobDto> UpdateJobAsync(Guid id, UpdateJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Updating job: {JobId}", id);

        var job = await _repository.GetByIdAsync(id);
        if (job == null)
        {
            _logger.LogWarning("Job not found for update: {JobId}", id);
            throw new JobNotFoundException(id);
        }

        // Validate and update cron expression if provided
        if (!string.IsNullOrWhiteSpace(request.CronExpression) && request.CronExpression != job.CronExpression)
        {
            var (isValid, error) = _cronValidator.ValidateCronExpression(request.CronExpression);
            if (!isValid)
            {
                _logger.LogWarning("Invalid cron expression during update: {CronExpression}", request.CronExpression);
                throw new InvalidCronExpressionException(request.CronExpression, error);
            }

            job.CronExpression = request.CronExpression;
            job.NextExecutionTime = _cronValidator.GetNextExecutionTime(request.CronExpression);
            _logger.LogInformation("Cron expression updated for job {JobId}", id);
        }

        // Update optional fields
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            job.Name = request.Name;
        }

        if (request.IsActive.HasValue)
        {
            job.IsActive = request.IsActive.Value;
            _logger.LogInformation("Job {JobId} status changed to {Status}", id, job.IsActive ? "Active" : "Inactive");
        }

        // Update retry policy if provided
        if (request.MaxRetries.HasValue || request.RetryIntervalSeconds.HasValue || request.BackoffMultiplier.HasValue)
        {
            if (request.MaxRetries.HasValue)
            {
                job.MaxRetries = request.MaxRetries.Value;
            }

            if (request.RetryIntervalSeconds.HasValue)
            {
                job.RetryIntervalSeconds = request.RetryIntervalSeconds.Value;
            }

            if (request.BackoffMultiplier.HasValue)
            {
                job.BackoffMultiplier = request.BackoffMultiplier.Value;
            }

            _logger.LogInformation("Retry policy updated for job {JobId}", id);
        }

        job.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(job);

        _logger.LogInformation("Job {JobId} updated successfully", id);
        return _mappingService.MapToJobDto(job);
    }

    public async Task<JobDto> RescheduleJobAsync(Guid id, RescheduleJobRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        _logger.LogInformation("Rescheduling job {JobId} with new cron: {NewCron}", id, request.NewCronExpression);

        var originalJob = await _repository.GetByIdAsync(id);
        if (originalJob == null)
        {
            _logger.LogWarning("Job not found for rescheduling: {JobId}", id);
            throw new JobNotFoundException(id);
        }

        // Validate new cron expression
        var (isValid, error) = _cronValidator.ValidateCronExpression(request.NewCronExpression);
        if (!isValid)
        {
            _logger.LogWarning("Invalid cron expression for reschedule: {CronExpression}", request.NewCronExpression);
            throw new InvalidCronExpressionException(request.NewCronExpression, error);
        }

        var newJob = new Job
        {
            Id = Guid.NewGuid(),
            Name = originalJob.Name,
            HandlerType = originalJob.HandlerType,
            HandlerName = originalJob.HandlerName,
            CronExpression = request.NewCronExpression,
            IsActive = true,
            MaxRetries = originalJob.MaxRetries,
            RetryIntervalSeconds = originalJob.RetryIntervalSeconds,
            BackoffMultiplier = originalJob.BackoffMultiplier,
            NextExecutionTime = _cronValidator.GetNextExecutionTime(request.NewCronExpression),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(newJob);

        _logger.LogInformation("Job rescheduled successfully: Original={OriginalJobId}, New={NewJobId}, Reason={Reason}", 
            originalJob.Id, newJob.Id, request.Reason);
        return _mappingService.MapToJobDto(newJob);
    }

    public async Task DeleteJobAsync(Guid id)
    {
        _logger.LogInformation("Deleting job: {JobId}", id);

        if (!await _repository.ExistsAsync(id))
        {
            _logger.LogWarning("Job not found for deletion: {JobId}", id);
            throw new JobNotFoundException(id);
        }

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Job {JobId} deleted successfully", id);
    }

    public async Task<bool> ToggleJobStatusAsync(Guid id)
    {
        _logger.LogInformation("Toggling status for job: {JobId}", id);

        var job = await _repository.GetByIdAsync(id);
        if (job == null)
        {
            _logger.LogWarning("Job not found for status toggle: {JobId}", id);
            throw new JobNotFoundException(id);
        }

        job.IsActive = !job.IsActive;
        job.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(job);

        _logger.LogInformation("Job {JobId} status toggled to {Status}", id, job.IsActive ? "Active" : "Inactive");
        return job.IsActive;
    }

    private string? GetHandlerName(string handlerType)
    {
        var handler = _handlerRegistry.Resolve(handlerType);
        return handler?.Name;
    }
}
