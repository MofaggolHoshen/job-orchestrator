using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JobOrchestrator.AspNetCore.Middleware;

/// <summary>
/// Optional middleware for ASP.NET Core applications that provides global exception handling.
/// 
/// This middleware catches all exceptions from Job Orchestrator and returns standardized HTTP responses.
/// It uses ExceptionToStatusCodeMapper to map Job Orchestrator exceptions to appropriate HTTP status codes 
/// (e.g., JobNotFoundException → 404).
/// 
/// NOTE: This middleware is OPTIONAL. You have three usage options:
/// 
/// 1. Use the complete middleware for automatic exception handling:
/// <code>
/// app.UseJobOrchestratorExceptionHandling();
/// </code>
/// 
/// 2. Use ExceptionToStatusCodeMapper in your own custom exception handler:
/// <code>
/// using JobOrchestrator.AspNetCore;
/// 
/// var statusCode = ExceptionToStatusCodeMapper.GetStatusCode(exception);
/// </code>
/// 
/// 3. Catch and handle Job Orchestrator exceptions directly in your code without using this middleware.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Creates a new instance of the exception handling middleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger instance for recording exceptions.</param>
    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware, catching any exceptions and converting them to standardized HTTP responses.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred in a Job Orchestrator operation");

        context.Response.ContentType = "application/json";

        var statusCode = ExceptionToStatusCodeMapper.GetStatusCode(exception);
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            Message = exception.Message ?? "An error occurred while processing your request.",
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.Body.WriteAsync(
            System.Text.Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            )
        );
    }
}

