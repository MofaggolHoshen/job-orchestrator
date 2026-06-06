using Microsoft.AspNetCore.Http;
using JobOrchestrator.Core.Exceptions;

namespace JobOrchestrator.AspNetCore;

/// <summary>
/// Helper class that maps Job Orchestrator exceptions to HTTP status codes.
/// This utility can be used in any exception handler or middleware to determine appropriate HTTP responses.
/// 
/// Usage scenarios:
/// - Custom exception handlers
/// - Exception filter attributes
/// - Custom middleware implementations
/// - Any HTTP-based consumer of Job Orchestrator
/// </summary>
public static class ExceptionToStatusCodeMapper
{
    /// <summary>
    /// Maps a Job Orchestrator exception to an HTTP status code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The appropriate HTTP status code (default: 500 Internal Server Error).</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await scheduler.ExecuteJobAsync(jobId);
    /// }
    /// catch (Exception ex)
    /// {
    ///     var statusCode = ExceptionToStatusCodeMapper.GetStatusCode(ex);
    ///     context.Response.StatusCode = statusCode;
    ///     await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    /// }
    /// </code>
    /// </example>
    public static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            JobNotFoundException => StatusCodes.Status404NotFound,
            InvalidCronExpressionException => StatusCodes.Status400BadRequest,
            JobHandlerNotFoundException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
