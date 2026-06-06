namespace JobOrchestrator.Core.DTOs;

/// <summary>
/// Request model for creating a new job.
/// </summary>
public class CreateJobRequest
{
    /// <summary>Display name for the job.</summary>
    public string Name { get; set; } = null!;

    /// <summary>Type identifier that determines which handler will process this job.</summary>
    public string HandlerType { get; set; } = null!;

    /// <summary>Cron expression defining the job's execution schedule.</summary>
    public string CronExpression { get; set; } = null!;

    /// <summary>Optional: Maximum number of retry attempts if the job fails.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Optional: Initial retry interval in seconds (default: 60).</summary>
    public int? RetryIntervalSeconds { get; set; }

    /// <summary>Optional: Backoff multiplier for exponential retry delays (default: 1.5).</summary>
    public decimal? BackoffMultiplier { get; set; }
}

