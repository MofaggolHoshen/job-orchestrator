# Job Orchestrator

A robust, enterprise-grade job scheduling and orchestration library for .NET 8.0+.

## Features

- ✅ **Cron-based Scheduling**: Standard CRON syntax for flexible job scheduling
- ✅ **Reliable Execution**: Automatic job execution with retry policies and exponential backoff
- ✅ **Persistent Storage**: In-memory (dev) or SQL Server (production) storage options
- ✅ **Dependency Injection**: Seamless integration with ASP.NET Core DI container
- ✅ **Exception Hierarchy**: Custom exceptions for precise error handling
- ✅ **Global Exception Middleware**: Optional ASP.NET Core middleware for HTTP error mapping
- ✅ **Structured Logging**: Full ILogger<T> integration
- ✅ **Async/Await**: Complete async support with CancellationToken
- ✅ **Production-Ready**: Thoroughly tested and documented

## Getting Started

See **[README_NUGET.md](./README_NUGET.md)** for complete documentation on:

- Installation and setup
- Creating job handlers
- Configuring the scheduler
- Exception handling
- Dependency injection
- Configuration options
- Troubleshooting

## Quick Example

```csharp
// 1. Create a handler
public class MyJobHandler : IJobHandler
{
    public string JobType => "MyJobType";
    public async Task ExecuteAsync(Job job, CancellationToken ct = default)
    {
        // Your job logic here
    }
}

// 2. Register in Program.cs
builder.Services
    .AddScheduler()
    .UseInMemoryStorage()
    .AddJobHandler<MyJobHandler>();

// 3. Schedule a job
await jobService.CreateJobAsync(new CreateJobRequest
{
    Name = "My Job",
    JobType = "MyJobType",
    CronExpression = "0 9 * * *"  // Daily at 9 AM
});
```

## Exception Handling

The library uses exceptions as the primary error mechanism. Always catch Job Orchestrator exceptions:

```csharp
try
{
    var job = await jobService.CreateJobAsync(request);
}
catch (InvalidCronExpressionException ex)
{
    // Handle invalid schedule
}
catch (JobHandlerNotFoundException ex)
{
    // Handle missing handler
}
catch (JobOrchestratorException ex)
{
    // Handle other errors
}
```

For ASP.NET Core, use the optional middleware:

```csharp
app.UseJobOrchestratorExceptionHandling();
```

## Project Structure

```
JobOrchestrator.Core/          Main library (NuGet package)
├── Services/                  Public APIs
├── Exceptions/                Custom exception types
├── Models/                    Domain models
├── DTOs/                      Request/response types
├── Middleware/                Optional ASP.NET Core middleware
└── Data/                      Database (EF Core)

JobOrchestrator.Dashboard/     Optional dashboard UI (Blazor)
```

## Architecture Decisions

### Exception-First Error Handling

- Library throws specific exceptions for error conditions
- Consumers must handle exceptions in try-catch blocks
- Middleware is optional for ASP.NET Core HTTP context mapping

### Public vs Internal APIs

- **Public**: IJobHandler, IJobService, ISchedulerService, exceptions, DTOs
- **Internal**: Data access, repositories, configuration details

### Storage Flexibility

- In-memory database for development/testing
- SQL Server for production persistence
- Easy to add other providers (PostgreSQL, etc.)

## Development

This repository is organized as follows:

- **REFACTORING.md** - Detailed refactoring notes and architectural improvements
- **README_NUGET.md** - Complete NuGet package documentation

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Packaging

```bash
dotnet pack JobOrchestrator.Core
```

## Contributing

Contributions are welcome! Please ensure:

- All exceptions are properly documented
- Public APIs have XML documentation
- Code follows SOLID principles
- Exception handling is explicit

## License

MIT License - see LICENSE file for details

## Support

For issues and questions, visit: https://github.com/MofaggolHoshen/job-orchestrator
