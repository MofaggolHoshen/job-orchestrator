namespace JobOrchestrator.Core.Models;

/// <summary>
/// Represents a scheduled job that runs according to a cron expression.
/// </summary>
public class Job
{
    /// <summary>Unique identifier for the job.</summary>
    public Guid Id { get; set; }

    /// <summary>Display name of the job.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Full type name (namespace.ClassName) of the handler that executes this job.</summary>
    public string HandlerType { get; set; } = null!;

    /// <summary>User-friendly name of the handler, from IJobHandler.Name property.</summary>
    public string HandlerName { get; set; } = null!;

    /// <summary>Cron expression defining the job's schedule (e.g., "0 0 * * *" for daily at midnight).</summary>
    public string CronExpression { get; set; } = null!;

    /// <summary>Indicates whether this job is active and eligible for execution.</summary>
    public bool IsActive { get; set; }

    /// <summary>The calculated UTC time of the next scheduled execution.</summary>
    public DateTime? NextExecutionTime { get; set; }

    /// <summary>UTC timestamp of the most recent successful execution.</summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>UTC timestamp when the job was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the most recent update to this job's configuration.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Navigation property: all execution records for this job.</summary>
    public ICollection<JobExecution> Executions { get; set; } = new List<JobExecution>();

    /// <summary>Maximum number of retry attempts after an initial failure.</summary>
    public int MaxRetries { get; set; }

    /// <summary>Initial delay in seconds between retry attempts.</summary>
    public int RetryIntervalSeconds { get; set; }

    /// <summary>Multiplier applied to retry interval for exponential backoff (e.g., 1.5 = 50% increase per retry).</summary>
    public decimal BackoffMultiplier { get; set; }
}
