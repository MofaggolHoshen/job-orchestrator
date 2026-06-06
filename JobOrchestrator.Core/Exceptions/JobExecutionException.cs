namespace JobOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a job execution fails unexpectedly.
/// 
/// This indicates the job handler threw an exception or encountered an error during execution.
/// Check the InnerException property for details about what went wrong.
/// 
/// The scheduler will automatically retry the job based on the MaxRetries and RetryIntervalSeconds
/// configuration unless the retry limit is exceeded.
/// 
/// Example handling:
/// <code>
/// try
/// {
///     await jobService.ExecuteJobAsync(jobId);
/// }
/// catch (JobExecutionException ex)
/// {
///     _logger.LogError(ex.InnerException, 
///         "Job {JobId} failed on attempt {Attempt}", 
///         ex.JobId, ex.AttemptNumber);
/// }
/// </code>
/// 
/// HTTP Status Code (if using ASP.NET Core middleware): 500 Internal Server Error
/// </summary>
public class JobExecutionException : JobOrchestratorException
{
    /// <summary>The ID of the job that failed to execute.</summary>
    public Guid JobId { get; }

    /// <summary>The attempt number when the failure occurred (useful for understanding retry progress).</summary>
    public int AttemptNumber { get; }

    /// <summary>
    /// Creates a new instance of JobExecutionException.
    /// </summary>
    /// <param name="jobId">The ID of the job that failed.</param>
    /// <param name="attemptNumber">The execution attempt number (1, 2, 3, etc.).</param>
    /// <param name="message">A message describing the execution failure.</param>
    /// <param name="innerException">The underlying exception that caused the failure.</param>
    public JobExecutionException(Guid jobId, int attemptNumber, string message, Exception? innerException = null)
        : base($"Job execution failed for job ID '{jobId}' (attempt {attemptNumber}): {message}", innerException ?? new Exception(message))
    {
        JobId = jobId;
        AttemptNumber = attemptNumber;
    }
}
