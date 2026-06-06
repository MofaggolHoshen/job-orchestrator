namespace JobOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a job cannot be found in the database.
/// 
/// This typically occurs when:
/// - Trying to retrieve a job by ID that doesn't exist
/// - Trying to update or delete a job that was already removed
/// - The job has been cleaned up from the scheduler
/// 
/// HTTP Status Code (if using ASP.NET Core middleware): 404 Not Found
/// </summary>
public class JobNotFoundException : JobOrchestratorException
{
    /// <summary>The ID of the job that was not found.</summary>
    public Guid JobId { get; }

    /// <summary>
    /// Creates a new instance of JobNotFoundException.
    /// </summary>
    /// <param name="jobId">The ID of the job that could not be found.</param>
    public JobNotFoundException(Guid jobId)
        : base($"Job with ID '{jobId}' was not found.")
    {
        JobId = jobId;
    }

    /// <summary>
    /// Creates a new instance of JobNotFoundException with additional context.
    /// </summary>
    /// <param name="jobId">The ID of the job that could not be found.</param>
    /// <param name="context">Additional context about why the job was not found.</param>
    public JobNotFoundException(Guid jobId, string context)
        : base($"Job with ID '{jobId}' was not found. {context}")
    {
        JobId = jobId;
    }
}
