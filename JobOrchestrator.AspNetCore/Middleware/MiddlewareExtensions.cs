using Microsoft.AspNetCore.Builder;

namespace JobOrchestrator.AspNetCore.Middleware;

/// <summary>
/// Extension methods for adding Job Orchestrator exception handling middleware to an ASP.NET Core application.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the Job Orchestrator global exception handling middleware to the pipeline.
    /// This middleware catches all exceptions thrown by Job Orchestrator services and converts them
    /// to standardized HTTP error responses.
    /// 
    /// This should be placed early in your middleware pipeline (typically after HTTPS redirection).
    /// 
    /// Example:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddScheduler().UseInMemoryStorage();
    /// 
    /// var app = builder.Build();
    /// app.UseJobOrchestratorExceptionHandling();
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseJobOrchestratorExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
