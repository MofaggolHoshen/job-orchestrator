# JobOrchestrator API

A consumer ASP.NET Core Web API that demonstrates integration with the JobOrchestrator.Core library and Dashboard components.

## Overview

The **Samples/JobOrchestrator.Api** is a fully functional ASP.NET Core Web API that:

- ✅ Consumes the **JobOrchestrator.Core** scheduler library
- ✅ Integrates the **JobOrchestrator.Dashboard** Blazor UI
- ✅ Provides REST API endpoints for job management
- ✅ Includes sample job handler implementation
- ✅ Uses in-memory storage by default (configurable to SQL Server)

## Project Structure

```
Samples/JobOrchestrator.Api/
├── Program.cs                           # Application entry point & configuration
├── appsettings.json                     # Configuration file
├── appsettings.Development.json         # Development configuration
├── JobOrchestrator.Api.csproj          # Project file
├── Controllers/
│   └── HandlerController.cs             # API endpoints for handler operations
├── Handlers/
│   └── SampleJobHandler.cs              # Sample job handler implementation
└── Extensions/
    ├── ApiServiceCollectionExtensions.cs    # DI service configuration
    └── ApiApplicationBuilderExtensions.cs   # Request pipeline configuration
```

## Dependencies

- **JobOrchestrator.Core**: Core scheduler and job handling services
- **JobOrchestrator.Dashboard**: Blazor UI components for the dashboard
- ASP.NET Core 8.0+
- Swashbuckle.AspNetCore (Swagger/OpenAPI documentation)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Building

```bash
cd c:\Users\mofag\Source\repos\JobOrchestrator
dotnet build
```

### Running the API

```bash
dotnet run --project Samples/JobOrchestrator.Api/JobOrchestrator.Api.csproj
```

The API will be available at:

- **API Base**: `https://localhost:7000` (or configured port)
- **Swagger/OpenAPI**: `https://localhost:7000/swagger/index.html`
- **Dashboard**: `https://localhost:7000/` (Razor component)

## API Endpoints

### Handler Operations

- **GET** `/api/handler` - Simple handler endpoint

  ```json
  {
    "message": "Handler executed successfully",
    "timestamp": "2026-05-31T02:20:00.000Z"
  }
  ```

- **GET** `/api/handler/execute` - Execute handler via controller
  ```json
  {
    "message": "Handler processed successfully",
    "timestamp": "2026-05-31T02:20:00.000Z",
    "status": "success"
  }
  ```

## Configuration

### Service Setup in Program.cs

```csharp
// Add scheduler with dashboard
builder.Services
    .AddJobOrchestratorApi()        // Adds scheduler + dashboard services
    .ConfigureInMemory()             // Use in-memory storage
    .AddJobHandler<SampleJobHandler>(); // Register custom handler
```

### Alternative: SQL Server Storage

```csharp
builder.Services
    .AddJobOrchestratorApi()
    .ConfigureSqlServer("Server=.;Database=JobOrchestrator;...connection string...");
```

### Register Custom Job Handlers

```csharp
builder.Services
    .AddJobOrchestratorApi()
    .ConfigureInMemory()
    .AddJobHandler<YourCustomHandler>()
    .AddJobHandler<AnotherHandler>();
```

## Adding Custom Job Handlers

Create a new handler by implementing `IJobHandler`:

```csharp
using JobOrchestrator.Core.Services;

public class MyCustomHandler : IJobHandler
{
    private readonly ILogger<MyCustomHandler> _logger;

    public string JobType => "MyJob"; // Unique handler identifier

    public MyCustomHandler(ILogger<MyCustomHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(object? context)
    {
        _logger.LogInformation("Processing MyJob with context: {0}", context);

        // Your job processing logic here
        await Task.Delay(100);

        _logger.LogInformation("MyJob completed successfully");
    }
}
```

Register it in Program.cs:

```csharp
builder.Services
    .AddJobOrchestratorApi()
    .ConfigureInMemory()
    .AddJobHandler<MyCustomHandler>();
```

## Extension Methods Reference

### Service Collection Extensions (`ApiServiceCollectionExtensions`)

- **AddJobOrchestratorApi()** - Registers scheduler and dashboard services
- **ConfigureInMemory(dbName)** - Uses in-memory database storage
- **ConfigureSqlServer(connectionString)** - Uses SQL Server for persistence

### Application Builder Extensions (`ApiApplicationBuilderExtensions`)

- **UseJobOrchestratorApi()** - Configures the HTTP pipeline with scheduler dashboard

## Architecture

The API follows a clean separation of concerns:

```
Controllers (REST endpoints)
    ↓
Services (Business logic)
    ↓
Core (Scheduler, Job Handlers)
    ↓
Data (Database context)
    ↓
Dashboard (Blazor UI components)
```

## Logging

Console and structured logging are configured in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

For development, see `appsettings.Development.json`.

## Testing the API

Using PowerShell:

```powershell
# Simple handler endpoint
Invoke-WebRequest -Uri "https://localhost:7000/api/handler" -SkipCertificateCheck

# Handler controller endpoint
Invoke-WebRequest -Uri "https://localhost:7000/api/handler/execute" -SkipCertificateCheck
```

Using curl:

```bash
# Simple handler endpoint
curl -k https://localhost:7000/api/handler

# Handler controller endpoint
curl -k https://localhost:7000/api/handler/execute
```

## Database Configuration

### In-Memory (Default)

Data is stored in memory and lost on application restart.

```csharp
.ConfigureInMemory("JobOrchestratorDb")
```

### SQL Server

Requires a SQL Server database connection string.

```csharp
.ConfigureSqlServer("Server=.;Database=JobOrchestrator;Trusted_Connection=true;")
```

## Notes

- The API automatically initializes the database schema on startup
- Job handlers are resolved per-execution with their own DI scope
- Dashboard requires JavaScript and is rendered server-side (Blazor Server)

## Troubleshooting

### Port Already in Use

If port 7000 is in use, configure a different port in `appsettings.json` or via environment variables.

### Database Initialization Fails

Ensure the database connection string is correct for SQL Server mode, or use in-memory storage for testing.

### Handler Not Found

Verify the handler is registered with `.AddJobHandler<YourHandler>()` in Program.cs.

## License

Part of the JobOrchestrator solution.
