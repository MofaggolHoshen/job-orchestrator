namespace JobOrchestrator.Core.Exceptions;

/// <summary>
/// Base exception for all Job Orchestrator related errors.
/// 
/// This exception hierarchy is the primary error handling mechanism for Job Orchestrator library consumers.
/// The library throws specific exception types to indicate different error conditions.
/// 
/// Exception Handling Guide for Consumers:
/// =====================================
/// 
/// 1. ALWAYS wrap Job Orchestrator API calls in try-catch blocks:
/// <code>
/// try
/// {
///     var job = await jobService.CreateJobAsync(request);
/// }
/// catch (InvalidCronExpressionException ex)
/// {
///     // Handle invalid cron: Return 400 Bad Request to user
///     _logger.LogWarning(ex, "Invalid cron expression: {Cron}", ex.CronExpression);
/// }
/// catch (JobHandlerNotFoundException ex)
/// {
///     // Handle missing handler: Job type not registered
///     _logger.LogError(ex, "Handler not found for job type: {JobType}", ex.JobType);
/// }
/// catch (JobOrchestratorException ex)
/// {
///     // Handle other orchestrator errors
///     _logger.LogError(ex, "Job Orchestrator error: {Message}", ex.Message);
/// }
/// </code>
/// 
/// 2. Exception Types and HTTP Status Codes for ASP.NET Core:
/// JobNotFoundException = 404 Not Found
/// InvalidCronExpressionException = 400 Bad Request
/// JobHandlerNotFoundException = 400 Bad Request
/// JobExecutionException = 500 Internal Server Error
/// JobOrchestratorException (other) = 500 Internal Server Error
/// 
/// 3. If using ASP.NET Core, optionally use the middleware:
/// <code>
/// app.UseJobOrchestratorExceptionHandling();
/// </code>
/// This automatically converts exceptions to appropriate HTTP responses.
/// 
/// 4. For non-ASP.NET Core applications, handle exceptions in your own try-catch blocks.
/// </summary>
public class JobOrchestratorException : Exception
{
    /// <summary>
    /// Creates a new instance of the JobOrchestratorException.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    public JobOrchestratorException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of the JobOrchestratorException with an inner exception.
    /// </summary>
    /// <param name="message">The error message describing what went wrong.</param>
    /// <param name="innerException">The underlying exception that caused this error.</param>
    public JobOrchestratorException(string message, Exception innerException)
        : base(message, innerException) { }
}
