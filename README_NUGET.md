# Job Orchestrator - NuGet Package

A robust, enterprise-grade job scheduling and orchestration library for .NET. Features include:

- ✅ Cron-based scheduling with standard CRON syntax
- ✅ Automatic job execution with configurable retry policies
- ✅ Exponential backoff support for retries
- ✅ Job persistence (in-memory or SQL Server)
- ✅ Dependency injection integration
- ✅ Structured logging with ILogger<T>
- ✅ Custom exception hierarchy for easy error handling
- ✅ ASP.NET Core middleware for HTTP exception mapping
- ✅ Fully async/await support with CancellationToken
- ✅ Production-ready and thoroughly documented

## Quick Start

### 1. Install the NuGet Package

```powershell
dotnet add package JobOrchestrator
```

### 2. Create a Job Handler

Implement `IJobHandler` to define what your job does:

```csharp
using JobOrchestrator.Core.Services;
using JobOrchestrator.Core.Models;

public class EmailNotificationHandler : IJobHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationHandler> _logger;

    public string JobType => "SendEmailNotification";

    public EmailNotificationHandler(IEmailService emailService, ILogger<EmailNotificationHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing email job: {JobId}", job.Id);

        // Parse job data if needed
        var recipientEmail = job.Data?["recipientEmail"] ?? throw new ArgumentException("Missing recipient email");

        // Execute the actual work
        await _emailService.SendEmailAsync(recipientEmail, cancellationToken);

        _logger.LogInformation("Email sent successfully for job {JobId}", job.Id);
    }
}
```

### 3. Configure in Program.cs

```csharp
using JobOrchestrator.Core.Services;
using JobOrchestrator.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add your job handlers and scheduler
builder.Services
    .AddScheduler()
    .UseInMemoryStorage()  // or .UseSqlServer(connectionString)
    .AddJobHandler<EmailNotificationHandler>()
    .Configure(options =>
    {
        options.PollingIntervalSeconds = 5;  // Check for due jobs every 5 seconds
        options.MaxParallelJobs = 3;         // Run up to 3 jobs in parallel
    });

var app = builder.Build();

// Optional: Add global exception handling middleware (ASP.NET Core only)
app.UseJobOrchestratorExceptionHandling();

app.MapControllers();
app.Run();
```

### 4. Schedule a Job

```csharp
[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpPost("schedule-email")]
    public async Task<IActionResult> ScheduleEmail([FromBody] ScheduleEmailRequest request)
    {
        try
        {
            var jobRequest = new CreateJobRequest
            {
                Name = "Send notification email",
                JobType = "SendEmailNotification",
                CronExpression = request.CronExpression,  // e.g., "0 9 * * *" for 9 AM daily
                MaxRetries = 3,
                RetryIntervalSeconds = 300,       // Wait 5 minutes between retries
                BackoffMultiplier = 2.0m,         // 2x, 4x, 8x wait time
                Data = new Dictionary<string, string>
                {
                    { "recipientEmail", request.Email }
                }
            };

            var job = await _jobService.CreateJobAsync(jobRequest);
            _logger.LogInformation("Job scheduled: {JobId}", job.Id);

            return CreatedAtAction(nameof(ScheduleEmail), new { jobId = job.Id }, job);
        }
        catch (InvalidCronExpressionException ex)
        {
            _logger.LogWarning(ex, "Invalid cron expression provided");
            return BadRequest(new { error = "Invalid cron expression", details = ex.Message });
        }
        catch (JobOrchestratorException ex)
        {
            _logger.LogError(ex, "Failed to schedule job");
            return StatusCode(500, new { error = "Failed to schedule job" });
        }
    }
}
```

## Exception Handling

Job Orchestrator uses exceptions as the primary error handling mechanism. Always wrap API calls in try-catch blocks:

### Exception Types

| Exception                        | HTTP Status | When Thrown                        |
| -------------------------------- | ----------- | ---------------------------------- |
| `JobNotFoundException`           | 404         | Job not found in database          |
| `InvalidCronExpressionException` | 400         | Invalid CRON syntax                |
| `JobHandlerNotFoundException`    | 400         | No handler registered for job type |
| `JobExecutionException`          | 500         | Job execution failed               |
| `JobOrchestratorException`       | 500         | Other unexpected errors            |

### Example: Complete Error Handling

```csharp
try
{
    var job = await _jobService.CreateJobAsync(request);
    return Ok(new { jobId = job.Id });
}
catch (InvalidCronExpressionException ex)
{
    // Invalid schedule expression - user error
    _logger.LogWarning(ex, "Invalid cron: {Expression}", ex.CronExpression);
    return BadRequest(new
    {
        error = "Invalid cron expression",
        expression = ex.CronExpression,
        details = ex.ValidationError
    });
}
catch (JobHandlerNotFoundException ex)
{
    // No handler registered - configuration issue
    _logger.LogError(ex, "Missing handler for job type: {JobType}", ex.JobType);
    return BadRequest(new
    {
        error = "Job handler not registered",
        jobType = ex.JobType
    });
}
catch (JobOrchestratorException ex)
{
    // Other orchestrator errors
    _logger.LogError(ex, "Job creation failed");
    return StatusCode(500, new { error = "Failed to create job" });
}
```

## CRON Expression Format

```
┌───────────── second (0 - 59)
│ ┌───────────── minute (0 - 59)
│ │ ┌───────────── hour (0 - 23)
│ │ │ ┌───────────── day of month (1 - 31)
│ │ │ │ ┌───────────── month (1 - 12)
│ │ │ │ │ ┌───────────── day of week (0 - 6) (Sunday to Saturday)
│ │ │ │ │ │
│ │ │ │ │ │
* * * * * *
```

### Common Examples

- `0 * * * *` → Every minute
- `0 0 * * *` → Daily at midnight
- `0 9 * * *` → Daily at 9 AM
- `0 9 * * MON` → Every Monday at 9 AM
- `0 0 1 * *` → Monthly on the 1st at midnight
- `0 */4 * * *` → Every 4 hours
- `0 9 * * MON-FRI` → Weekdays at 9 AM

## Configuration Options

```csharp
builder.Services.AddScheduler().Configure(options =>
{
    // How often the scheduler checks for due jobs (seconds)
    options.PollingIntervalSeconds = 5;

    // Maximum number of jobs to execute in parallel
    options.MaxParallelJobs = 3;
});
```

## Storage Options

### In-Memory (Development/Testing)

```csharp
builder.Services
    .AddScheduler()
    .UseInMemoryStorage("MySchedulerDb");  // Data lost on restart
```

### SQL Server (Production)

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services
    .AddScheduler()
    .UseSqlServer(connectionString);  // Persists data
```

## Dependency Injection

Job handlers are resolved with their own DI scope per execution, allowing them to have scoped dependencies:

```csharp
public class DatabaseBackupHandler : IJobHandler
{
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory;

    public string JobType => "DatabaseBackup";

    public DatabaseBackupHandler(IDbContextFactory<MyDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        // Each job execution gets its own DbContext
        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.ExecuteSqlAsync(new FormattableString($"BACKUP DATABASE..."), cancellationToken);
    }
}
```

## Without ASP.NET Core

If you're using Job Orchestrator in a non-ASP.NET Core application (Console, Windows Service, etc.), you don't use the middleware. Instead, handle exceptions directly:

```csharp
// In a Console app or background service
try
{
    var job = await jobService.CreateJobAsync(request);
    Console.WriteLine($"Job created: {job.Id}");
}
catch (InvalidCronExpressionException ex)
{
    Console.WriteLine($"Invalid schedule: {ex.CronExpression}");
    // Handle as needed
}
catch (JobOrchestratorException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    // Handle as needed
}
```

## Logging

The library uses `ILogger<T>` for structured logging. Configure it in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "JobOrchestrator": "Information",
      "JobOrchestrator.Core.Services.SchedulerService": "Debug"
    }
  }
}
```

## Performance Considerations

- **Polling Interval**: Lower values (1-5 seconds) for responsive scheduling; higher values (30+ seconds) for lower CPU usage
- **Max Parallel Jobs**: Balance between throughput and system resources
- **Retry Strategy**: Use exponential backoff to avoid thundering herd
- **Job Handler Performance**: Each handler runs in its own scope; long-running jobs block other parallel jobs

## Troubleshooting

### "No handler registered for job type"

Register your handler during startup:

```csharp
builder.Services.AddScheduler().AddJobHandler<YourHandler>();
```

### "Invalid cron expression"

Check your CRON syntax. Use an online CRON validator or review the examples above.

### Jobs not executing

- Check that the scheduler service is running (should be auto-hosted)
- Verify `PollingIntervalSeconds` isn't set too high
- Check logs for execution errors
- Ensure job handler is registered

### Performance Issues

- Increase `PollingIntervalSeconds` to reduce database load
- Reduce `MaxParallelJobs` if database/resources are overwhelmed
- Profile job handler execution time
- Consider dedicated scheduler instance for high-volume scenarios

## License

This library is provided under the MIT License.

## Support

For issues, feature requests, or documentation, visit:
https://github.com/MofaggolHoshen/job-orchestrator
