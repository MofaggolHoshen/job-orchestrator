namespace JobOrchestrator.Core.DTOs;

/// <summary>
/// Request model for rescheduling an existing job with a new cron expression.
/// Creates a new job and tracks the relationship to the original job.
/// </summary>
public class RescheduleJobRequest
{
    /// <summary>The new cron expression for the rescheduled job.</summary>
    public string NewCronExpression { get; set; } = null!;

    /// <summary>Reason for the reschedule (e.g., "Moved to off-peak hours").</summary>
    public string Reason { get; set; } = null!;
}

