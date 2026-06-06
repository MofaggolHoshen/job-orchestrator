namespace JobOrchestrator.Core.Constants;

/// <summary>
/// Centralized error messages for consistent error reporting.
/// </summary>
public static class ErrorMessages
{
    public const string JobNotFound = "The requested job was not found.";
    public const string InvalidJobName = "Job name cannot be null, empty, or whitespace.";
    public const string InvalidJobType = "Job type cannot be null, empty, or whitespace.";
    public const string InvalidCronExpression = "The provided cron expression is invalid.";
    public const string CannotCalculateNextExecution = "Cannot calculate next execution time from the cron expression.";
    public const string InvalidMaxRetries = "Max retries must be greater than or equal to 0.";
    public const string InvalidRetryInterval = "Retry interval must be greater than 0.";
    public const string InvalidBackoffMultiplier = "Backoff multiplier must be greater than 0.";
    public const string NoHandlerRegistered = "No handler is registered for the specified job type.";
    public const string NextExecutionTimeNotCalculated = "Next execution time could not be calculated.";
}
