namespace JobOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when no handler is registered for a specific job type.
/// 
/// This occurs when:
/// - A job with a certain type is scheduled but no IJobHandler is registered for it
/// - The scheduler tries to execute a job type that has no registered handler
/// 
/// To fix: Register a handler by calling services.AddJobHandler&lt;THandler&gt;() during setup.
/// 
/// Example:
/// <code>
/// builder.Services
///     .AddScheduler()
///     .UseInMemoryStorage()
///     .AddJobHandler&lt;MyJobHandler&gt;();
/// </code>
/// 
/// HTTP Status Code (if using ASP.NET Core middleware): 400 Bad Request
/// </summary>
public class JobHandlerNotFoundException : JobOrchestratorException
{
    /// <summary>The job type that has no registered handler.</summary>
    public string JobType { get; }

    /// <summary>
    /// Creates a new instance of JobHandlerNotFoundException.
    /// </summary>
    /// <param name="jobType">The job type that has no registered handler.</param>
    public JobHandlerNotFoundException(string jobType)
        : base($"No handler registered for job type '{jobType}'. Register a handler using AddJobHandler<THandler>().")
    {
        JobType = jobType;
    }
}
