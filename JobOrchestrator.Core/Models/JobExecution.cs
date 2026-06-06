namespace JobOrchestrator.Core.Models;

/// <summary>
/// Records a single execution attempt of a job.
/// </summary>
public class JobExecution
{
    /// <summary>Unique identifier for this execution record.</summary>
    public Guid Id { get; set; }

    /// <summary>Foreign key to the Job that was executed.</summary>
    public Guid JobId { get; set; }

    /// <summary>Timestamp when the execution started.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Timestamp when the execution completed (null if still running).</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Current status of the execution.</summary>
    public JobExecutionStatus Status { get; set; }

    /// <summary>Error message if the execution failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Which attempt this was: 1 for initial execution, 2+ for retries.</summary>
    public int ExecutionAttempt { get; set; }

    /// <summary>Navigation property to the associated job.</summary>
    public Job Job { get; set; } = null!;
}

/// <summary>
/// Enumeration of possible execution statuses.
/// </summary>
public enum JobExecutionStatus
{
    /// <summary>The execution is queued but not yet started.</summary>
    Pending = 0,

    /// <summary>The execution is currently in progress.</summary>
    Running = 1,

    /// <summary>The execution completed successfully.</summary>
    Completed = 2,

    /// <summary>The execution failed permanently after all retries.</summary>
    Failed = 3,

    /// <summary>The execution failed but will be retried.</summary>
    Retrying = 4
}

