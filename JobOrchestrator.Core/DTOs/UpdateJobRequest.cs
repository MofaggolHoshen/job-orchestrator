namespace JobOrchestrator.Core.DTOs;

/// <summary>
/// Request model for updating an existing job.
/// Only provided fields will be updated; null/default values are skipped.
/// </summary>
public class UpdateJobRequest
{
    /// <summary>Optional: New display name for the job.</summary>
    public string? Name { get; set; }

    /// <summary>Optional: New cron expression for the job's schedule.</summary>
    public string? CronExpression { get; set; }

    /// <summary>Optional: Enable or disable the job.</summary>
    public bool? IsActive { get; set; }

    /// <summary>Optional: Update maximum number of retry attempts.</summary>
    public int? MaxRetries { get; set; }

    /// <summary>Optional: Update retry interval in seconds.</summary>
    public int? RetryIntervalSeconds { get; set; }

    /// <summary>Optional: Update backoff multiplier for retries.</summary>
    public decimal? BackoffMultiplier { get; set; }
}

