# JobOrchestrator.AspNetCore

**Optional ASP.NET Core integration package for [JobOrchestrator](https://github.com/MofaggolHoshen/job-orchestrator)** providing exception handling utilities and optional middleware.

## Overview

`JobOrchestrator.AspNetCore` is a complementary package to the core `JobOrchestrator` library that provides ASP.NET Core-specific functionality:

- 🛡️ **Global Exception Handling Middleware** (Optional) - Catches Job Orchestrator exceptions and converts them to standardized HTTP responses
- 📊 **HTTP Status Code Mapper** - Reusable utility to map exceptions to HTTP status codes
- 📝 **Standardized Error Responses** - Consistent DTO for error formatting
- 🔧 **Flexible Integration** - Use the complete middleware, the mapper alone, or build your own

## Installation

Install via NuGet:

```bash
dotnet add package JobOrchestrator.AspNetCore
```

Or using Package Manager:

```powershell
Install-Package JobOrchestrator.AspNetCore
```

## Usage Options

### Option 1: Use Complete Middleware (Simplest)

Add the optional exception handling middleware for automatic exception handling:

```csharp
using JobOrchestrator.AspNetCore.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add scheduler and other services
builder.Services.AddScheduler().UseInMemoryStorage();

var app = builder.Build();

// Add the middleware EARLY in the pipeline
app.UseJobOrchestratorExceptionHandling();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
```

### Option 2: Use Mapper in Custom Handler (Flexible)

Use `ExceptionToStatusCodeMapper` to map exceptions in your own error handling logic:

```csharp
using JobOrchestrator.AspNetCore;

// In your custom exception handler, filter, or middleware:
try
{
    await scheduler.ExecuteJobAsync(jobId);
}
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

### Option 3: Use ErrorResponse DTO

Use the `ErrorResponse` class in your own exception handlers:

```csharp
using JobOrchestrator.AspNetCore;

catch (JobNotFoundException ex)
{
    var error = new ErrorResponse
    {
        Message = ex.Message,
        StatusCode = 404,
        Timestamp = DateTime.UtcNow
    };

    return Problem(detail: error.Message, statusCode: error.StatusCode);
}
```

## Reusable Components

This package provides components that you can use independently:

### ExceptionToStatusCodeMapper

Maps Job Orchestrator exceptions to HTTP status codes. Perfect for:
- Custom exception handlers
- Exception filter attributes
- Custom middleware implementations
- Any exception handling scenario

```csharp
using JobOrchestrator.AspNetCore;

public class JobExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is JobOrchestratorException ex)
        {
            var statusCode = ExceptionToStatusCodeMapper.GetStatusCode(ex);
            context.Result = new ObjectResult(
                new ErrorResponse 
                { 
                    Message = ex.Message,
                    StatusCode = statusCode,
                    Timestamp = DateTime.UtcNow
                }
            ) { StatusCode = statusCode };
        }
    }
}
```

### ErrorResponse

A standard DTO for error responses:

```csharp
var error = new ErrorResponse
{
    Message = "Something went wrong",
    StatusCode = 500,
    Timestamp = DateTime.UtcNow
};
```

The middleware automatically maps the following Job Orchestrator exceptions to HTTP status codes:

| Exception | HTTP Status |
|-----------|-----------|
| `JobNotFoundException` | 404 Not Found |
| `InvalidCronExpressionException` | 400 Bad Request |
| `JobHandlerNotFoundException` | 400 Bad Request |
| `ArgumentException` | 400 Bad Request |
| Other exceptions | 500 Internal Server Error |

## Error Response Format

All errors are returned in a standardized JSON format:

```json
{
  "message": "Job with ID '00000000-0000-0000-0000-000000000000' was not found.",
  "statusCode": 404,
  "timestamp": "2024-12-28T10:30:45.123Z"
}
```

- **message**: Description of the error
- **statusCode**: HTTP status code
- **timestamp**: UTC timestamp when the error occurred

## Middleware Placement

The exception handling middleware should be placed **early** in your middleware pipeline, typically:

✅ **Good placement**:
```csharp
app.UseJobOrchestratorExceptionHandling();  // Early
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

❌ **Avoid placing after auth/routing**:
```csharp
app.UseAuthorization();
app.UseJobOrchestratorExceptionHandling();  // Too late - won't catch auth exceptions
app.MapControllers();
```

## Custom Error Handling

If you need custom error handling beyond what the middleware provides, you can:

1. **Skip the middleware** - Don't call `UseJobOrchestratorExceptionHandling()`
2. **Catch exceptions directly**:
   ```csharp
   using JobOrchestrator.Core.Exceptions;

   try
   {
	   await scheduler.ExecuteJobAsync(jobId);
   }
   catch (JobNotFoundException ex)
   {
	   return NotFound(new { error = ex.Message });
   }
   ```

## Dependencies

- `Microsoft.AspNetCore.Http` 2.2.2 or higher
- `JobOrchestrator.Core` 1.0.0 or higher
- .NET 8.0 or higher

## Optional vs Required

This package is **completely optional**. 

- ✅ Use it if you're building an **ASP.NET Core application** and want standardized exception handling
- ❌ Skip it if you're using Job Orchestrator in:
  - Console applications
  - Windows Services
  - Azure Functions
  - gRPC services
  - Or prefer your own error handling

## Documentation

For more information about Job Orchestrator itself, see the [main repository](https://github.com/MofaggolHoshen/job-orchestrator).

## License

MIT License - See LICENSE file for details.
