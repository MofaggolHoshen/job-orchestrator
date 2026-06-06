# Job Orchestrator

A robust, enterprise-grade **background job scheduling and orchestration library** for .NET 8.0+. Run any code on a schedule—send emails, process data, run maintenance tasks, or execute any async operation with reliable execution, automatic retries, and persistent storage.

## Overview

Job Orchestrator provides a complete solution for scheduling and executing background jobs in .NET applications. Whether you're building an ASP.NET Core API, a Windows Service, or a Console application, Job Orchestrator handles job scheduling, execution, retries, and persistent tracking with minimal configuration.

### Key Capabilities

- ✅ **Cron-based Scheduling** - Standard CRON syntax for flexible job schedules
- ✅ **Reliable Execution** - Automatic job execution with configurable retry policies and exponential backoff
- ✅ **Flexible Storage** - In-memory (dev) or SQL Server (production) with EF Core migrations
- ✅ **Dependency Injection** - First-class DI integration with .NET built-in container
- ✅ **Exception Hierarchy** - Strongly-typed exceptions for precise error handling
- ✅ **Structured Logging** - Full `ILogger<T>` integration for observability
- ✅ **Complete Async** - Full async/await support with CancellationToken propagation
- ✅ **ASP.NET Core Extensions** - Optional middleware, exception mapping, and UI dashboard
- ✅ **Production-Ready** - Thoroughly tested, documented, and battle-tested

## What's in This Repository

### Core Library: `JobOrchestrator.Core`
The heart of the system. Provides job scheduling, execution, storage, and all public APIs.

- **Job Service**: Create, retrieve, update, and execute jobs
- **Scheduler Service**: Background service that polls and executes due jobs
- **Storage Layer**: Abstract storage with in-memory and SQL Server implementations
- **Exception Types**: Custom exceptions for each error condition
- **Handler Registry**: Extensible registry for job type handlers
- **Cron Validation**: CRON expression parsing and validation

### Extension: `JobOrchestrator.AspNetCore` (Optional)
ASP.NET Core-specific utilities for HTTP error mapping and middleware.

- Global exception handling middleware for automatic HTTP error responses
- Exception-to-HTTP status code mapper
- Standardized error response DTO
- Optional—skip it if not using ASP.NET Core

### Extension: `JobOrchestrator.Dashboard` (Optional)
Blazor-based UI for monitoring and managing jobs in real-time.

- Visual job monitoring
- Job creation and scheduling UI
- Execution history and logs
- Optional—complementary to the core library

### Sample: `Samples/JobOrchestrator.Api`
Complete working example showing how to use the library with ASP.NET Core, handlers, scheduling, and the dashboard.

---

## Quick Start: Core Setup

### 1. Install NuGet Package

```bash
dotnet add package JobOrchestrator.Core
```

### 2. Create a Job Handler

Implement `IJobHandler` for your job logic:

```csharp
using JobOrchestrator.Core.Services;
using JobOrchestrator.Core.Models;

public class SendEmailHandler : IJobHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailHandler> _logger;

    public string Name => "Send Email Handler";

    public SendEmailHandler(IEmailService emailService, ILogger<SendEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing job: {JobId}", job.Id);
        
        var email = job.Data?["email"] ?? throw new ArgumentException("Missing email");
        await _emailService.SendAsync(email, cancellationToken);
        
        _logger.LogInformation("Job completed: {JobId}", job.Id);
    }
}
```

### 3. Register in Program.cs

```csharp
using JobOrchestrator.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddScheduler()
    .UseInMemoryStorage()  // or .UseSqlServer(connectionString)
    .AddJobHandler<SendEmailHandler>();

var app = builder.Build();
app.Run();
```

### 4. Schedule a Job

```csharp
[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    [HttpPost("schedule-email")]
    public async Task<IActionResult> ScheduleEmail(string email)
    {
        try
        {
            var job = await _jobService.CreateJobAsync(new CreateJobRequest
            {
                Name = "Send Email",
                JobType = "SendEmail",
                CronExpression = "0 9 * * *",  // Daily at 9 AM
                Data = new Dictionary<string, string> { { "email", email } }
            });
            return Ok(new { jobId = job.Id });
        }
        catch (InvalidCronExpressionException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

## Core Configuration

### In-Memory Storage (Development)

```csharp
builder.Services
    .AddScheduler()
    .UseInMemoryStorage()  // Data lost on restart
    .AddJobHandler<MyHandler>();
```

### SQL Server Storage (Production)

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services
    .AddScheduler()
    .UseSqlServer(connectionString)  // Persistent storage
    .AddJobHandler<MyHandler>();
```

### Configuration Options

```csharp
builder.Services
    .AddScheduler()
    .UseInMemoryStorage()
    .Configure(options =>
    {
        // How often to check for due jobs (seconds)
        options.PollingIntervalSeconds = 5;
        
        // Maximum jobs to execute in parallel
        options.MaxParallelJobs = 3;
    })
    .AddJobHandler<MyHandler>();
```

### Retry Policy

Configure retry behavior per job:

```csharp
var job = await _jobService.CreateJobAsync(new CreateJobRequest
{
    Name = "Retry Example",
    JobType = "MyHandler",
    CronExpression = "0 * * * *",
    
    // Retry configuration
    MaxRetries = 3,              // Retry up to 3 times
    RetryIntervalSeconds = 300,  // 5 minutes between retries
    BackoffMultiplier = 2.0m     // Double wait time each retry (5m, 10m, 20m)
});
```

---

## Extensions

### ASP.NET Core Integration: `JobOrchestrator.AspNetCore`

Automatic HTTP exception handling and error response mapping.

#### Installation

```bash
dotnet add package JobOrchestrator.AspNetCore
```

#### Use Complete Middleware

```csharp
using JobOrchestrator.AspNetCore.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScheduler().UseInMemoryStorage();

var app = builder.Build();

// Add early in middleware pipeline
app.UseJobOrchestratorExceptionHandling();

app.MapControllers();
app.Run();
```

**Result**: All Job Orchestrator exceptions automatically map to HTTP responses:
- `JobNotFoundException` → 404 Not Found
- `InvalidCronExpressionException` → 400 Bad Request
- `JobHandlerNotFoundException` → 400 Bad Request
- Other exceptions → 500 Internal Server Error

#### Custom Error Handling

Use the mapper in your own exception handling:

```csharp
using JobOrchestrator.AspNetCore;

catch (Exception ex)
{
    var statusCode = ExceptionToStatusCodeMapper.GetStatusCode(ex);
    var errorResponse = new ErrorResponse
    {
        Message = ex.Message,
        StatusCode = statusCode,
        Timestamp = DateTime.UtcNow
    };
    
    context.Response.StatusCode = statusCode;
    await context.Response.WriteAsJsonAsync(errorResponse);
}
```

**Error Response Format:**
```json
{
  "message": "Job with ID 'abc123' was not found.",
  "statusCode": 404,
  "timestamp": "2024-12-28T10:30:45.123Z"
}
```

### Dashboard: `JobOrchestrator.Dashboard`

Visual monitoring and job management UI built with Blazor.

#### Installation

```bash
dotnet add package JobOrchestrator.Dashboard
```

#### Enable Dashboard

```csharp
using JobOrchestrator.Dashboard;

builder.Services
    .AddScheduler()
    .UseInMemoryStorage()
    .WithDashboard()  // Add dashboard UI
    .AddJobHandler<MyHandler>();

var app = builder.Build();

// Dashboard is now available at /jobs-dashboard
app.Run();
```

**Features:**
- Real-time job monitoring
- Job creation and scheduling form
- Execution history and logs
- Status indicators (pending, running, completed, failed)

---

## Exception Handling

The library uses **exceptions as the primary error mechanism**. Always wrap API calls in try-catch blocks:

```csharp
try
{
    var job = await _jobService.CreateJobAsync(request);
}
catch (InvalidCronExpressionException ex)
{
    // Invalid CRON syntax
    _logger.LogWarning("Invalid cron: {Expression}", ex.CronExpression);
}
catch (JobHandlerNotFoundException ex)
{
    // No handler registered for job type
    _logger.LogError("Missing handler for: {JobType}", ex.JobType);
}
catch (JobNotFoundException ex)
{
    // Job doesn't exist
    _logger.LogError("Job not found: {JobId}", ex.JobId);
}
catch (JobOrchestratorException ex)
{
    // Other orchestrator errors
    _logger.LogError(ex, "Orchestrator error");
}
```

### Exception Types

| Exception | When Thrown |
|-----------|-----------|
| `JobNotFoundException` | Job not found by ID |
| `InvalidCronExpressionException` | Invalid CRON expression syntax |
| `JobHandlerNotFoundException` | No handler registered for job type |
| `JobExecutionException` | Job handler execution failed |
| `JobOrchestratorException` | Other unexpected errors |

---

## Project Structure

```
JobOrchestrator/
├── JobOrchestrator.Core/              Core library (NuGet package)
│   ├── Services/                      Public APIs (IJobService, ISchedulerService)
│   ├── Exceptions/                    Custom exception types
│   ├── Models/                        Domain models (Job, JobStatus)
│   ├── DTOs/                          Request/response types
│   ├── Data/                          EF Core DbContext
│   ├── Repositories/                  Job persistence layer
│   ├── Migrations/                    Database migrations
│   └── Extensions/                    Fluent registration helpers
│
├── JobOrchestrator.AspNetCore/        Optional ASP.NET Core integration
│   ├── Middleware/                    Exception handling middleware
│   └── Utilities/                     HTTP mappers and DTOs
│
├── JobOrchestrator.Dashboard/         Optional Blazor UI
│   ├── Components/                    Dashboard components
│   └── wwwroot/                       Static assets
│
├── Samples/
│   └── JobOrchestrator.Api/           Complete example API
│
└── Documentation/
    ├── README.md                      (You are here)
    ├── README_NUGET.md                Complete NuGet package docs
    ├── REFACTORING.md                 Architecture and design decisions
    └── AGENTS.md                      For AI coding agents
```

---

## Complete Example

See **[Samples/JobOrchestrator.Api](./Samples/JobOrchestrator.Api)** for a fully working example that includes:

- Multiple job handlers
- Cron-based scheduling
- ASP.NET Core API endpoints
- Dashboard integration
- Exception handling

---

## Building and Packaging

### Build

```bash
dotnet build
```

### Pack NuGet Package

```bash
dotnet pack JobOrchestrator.Core
```

---

## Documentation

For detailed information, see:

- **[README_NUGET.md](./README_NUGET.md)** - Complete usage guide, examples, and troubleshooting
- **[REFACTORING.md](./REFACTORING.md)** - Architecture decisions and design patterns
- **[NUGET_REFACTORING.md](./NUGET_REFACTORING.md)** - Packaging and distribution notes

---

## Key Features in Detail

### Dependency Injection

All handlers receive their dependencies through constructor injection:

```csharp
public class MyHandler : IJobHandler
{
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MyHandler> _logger;

    public string Name => "My Custom Handler";

    public MyHandler(
        IDbContextFactory<MyDbContext> dbContextFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<MyHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        // Each execution gets a scoped DbContext
        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        using var client = _httpClientFactory.CreateClient();
        
        // Your logic here
    }
}
```

### CRON Expression Format

```
* * * * * *
│ │ │ │ │ │
│ │ │ │ │ └── Day of week (0-6, Sunday=0)
│ │ │ │ └───── Month (1-12)
│ │ │ └─────── Day of month (1-31)
│ │ └───────── Hour (0-23)
│ └─────────── Minute (0-59)
└───────────── Second (0-59)
```

**Common Examples:**
- `0 * * * *` → Every minute
- `0 0 * * *` → Daily at midnight
- `0 9 * * *` → Daily at 9 AM
- `0 9 * * MON-FRI` → Weekdays at 9 AM
- `0 */4 * * *` → Every 4 hours
- `0 0 1 * *` → Monthly on the 1st

### Structured Logging

Configure logging in `appsettings.json`:

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

---

## Architecture Highlights

### Exception-First Design

- Library throws specific exceptions for error conditions
- No result wrappers or "success" booleans—exceptions carry all error information
- Explicit try-catch encourages proper error handling

### Clean Boundaries

- **Public**: `IJobHandler`, `IJobService`, `ISchedulerService`, exceptions, DTOs
- **Internal**: Repositories, DbContext, migrations, configuration details

### Storage Abstraction

- Swap between in-memory and SQL Server with a single line
- Easy to add PostgreSQL, MySQL, or other providers
- Migrations provided for SQL Server

---

## Contributing

Contributions are welcome! When contributing:

- Keep changes focused and minimal
- Maintain nullable reference types enabled
- Update XML documentation for public APIs
- Follow exception-first error handling pattern
- Update [README_NUGET.md](./README_NUGET.md) and sample code if changing public APIs

---

## License

MIT License - see [LICENSE](./LICENSE) file for details

---

## Support & Community

**Found a bug or have a question?**

Visit: https://github.com/MofaggolHoshen/job-orchestrator

**Issues**: File bug reports or feature requests on GitHub  
**Discussions**: Ask questions and share feedback  
**Pull Requests**: Submit improvements and fixes
