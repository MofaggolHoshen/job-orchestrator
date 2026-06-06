namespace JobOrchestrator.Core.DTOs;

/// <summary>
/// Data transfer object representing a job.
/// </summary>
public class JobDto
{
    /// <summary>Unique identifier for the job.</summary>
    public Guid Id { get; set; }

    /// <summary>Display name of the job.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Full type name (namespace.ClassName) of the handler that executes this job.</summary>
    public string HandlerType { get; set; } = null!;

    /// <summary>User-friendly name of the handler, from IJobHandler.Name property.</summary>
    public string HandlerName { get; set; } = null!;

    /// <summary>Cron expression defining the job's schedule.</summary>
    public string CronExpression { get; set; } = null!;

    /// <summary>Indicates whether the job is currently active and eligible for execution.</summary>
    public bool IsActive { get; set; }

    /// <summary>The calculated time when the job will next execute.</summary>
    public DateTime? NextExecutionTime { get; set; }

    /// <summary>The timestamp of the most recent successful execution.</summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>Timestamp when the job was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Timestamp of the last update to the job configuration.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Retry policy configuration for this job, if configured.</summary>
    public RetryPolicyDto? RetryPolicy { get; set; }

    /// <summary>Recent execution history (last 5 executions by default).</summary>
    public List<JobExecutionDto> RecentExecutions { get; set; } = new();
}

/// <summary>
/// Data transfer object for retry policy configuration.
/// </summary>
public class RetryPolicyDto
{
    /// <summary>Maximum number of retry attempts after initial failure.</summary>
    public int MaxRetries { get; set; }

    /// <summary>Initial retry interval in seconds.</summary>
    public int RetryIntervalSeconds { get; set; }

    /// <summary>Multiplier applied to retry interval for exponential backoff.</summary>
    public decimal BackoffMultiplier { get; set; }
}

/// <summary>
/// Data transfer object for job execution records.
/// </summary>
public class JobExecutionDto
{
    /// <summary>Unique identifier for this execution record.</summary>
    public Guid Id { get; set; }

    /// <summary>Timestamp when execution started.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Timestamp when execution ended (null if still running).</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Current status of the execution (Running, Completed, Failed, Retrying).</summary>
    public string Status { get; set; } = null!;

    /// <summary>Error message if execution failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Which attempt number this was (1 for initial, 2+ for retries).</summary>
    public int ExecutionAttempt { get; set; }
}

