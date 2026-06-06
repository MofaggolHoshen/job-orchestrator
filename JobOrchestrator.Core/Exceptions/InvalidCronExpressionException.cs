namespace JobOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a cron expression is invalid or cannot be parsed.
/// 
/// This occurs when:
/// - The cron expression doesn't follow the standard cron syntax
/// - The expression contains invalid field values
/// - The expression cannot be evaluated
/// 
/// Cron Expression Format: Second Minute Hour DayOfMonth Month DayOfWeek [Year]
/// Example valid expressions:
/// - "0 * * * *" → Every minute
/// - "0 0 * * *" → Daily at midnight
/// - "0 0 0 1 *" → Monthly on the 1st
/// - "0 9 * * MON" → Every Monday at 9 AM
/// 
/// HTTP Status Code (if using ASP.NET Core middleware): 400 Bad Request
/// </summary>
public class InvalidCronExpressionException : JobOrchestratorException
{
    /// <summary>The cron expression that failed validation.</summary>
    public string CronExpression { get; }

    /// <summary>Additional validation error details, if available.</summary>
    public string? ValidationError { get; }

    /// <summary>
    /// Creates a new instance of InvalidCronExpressionException.
    /// </summary>
    /// <param name="cronExpression">The invalid cron expression.</param>
    /// <param name="validationError">Optional detailed validation error message.</param>
    public InvalidCronExpressionException(string cronExpression, string? validationError = null)
        : base($"Invalid cron expression: '{cronExpression}'. {validationError ?? "The expression does not follow cron syntax."}")
    {
        CronExpression = cronExpression;
        ValidationError = validationError;
    }
}
