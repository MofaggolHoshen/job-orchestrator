using CronExpressionDescriptor;
using Microsoft.Extensions.Logging;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Service for validating and processing cron expressions.
/// </summary>
public interface ICronValidationService
{
    /// <summary>
    /// Validates a cron expression.
    /// </summary>
    /// <returns>A tuple containing validation result and error message if invalid.</returns>
    (bool IsValid, string? ErrorMessage) ValidateCronExpression(string cronExpression);

    /// <summary>
    /// Calculates the next execution time based on a cron expression.
    /// </summary>
    DateTime? GetNextExecutionTime(string cronExpression, DateTime? baseTime = null);

    /// <summary>
    /// Gets a human-readable description of a cron expression.
    /// </summary>
    string GetCronDescription(string cronExpression);
}

/// <inheritdoc />
public class CronValidationService : ICronValidationService
{
    private readonly ILogger<CronValidationService> _logger;

    public CronValidationService(ILogger<CronValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public (bool IsValid, string? ErrorMessage) ValidateCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            _logger.LogWarning("Cron expression validation failed: expression is empty");
            return (false, "Cron expression cannot be empty.");
        }

        try
        {
            var descriptor = ExpressionDescriptor.GetDescription(cronExpression);
            _logger.LogDebug("Cron expression validated successfully: {Expression}", cronExpression);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cron expression validation failed for: {Expression}", cronExpression);
            return (false, $"Invalid Cron expression: {ex.Message}");
        }
    }

    public DateTime? GetNextExecutionTime(string cronExpression, DateTime? baseTime = null)
    {
        try
        {
            var baseDateTime = baseTime ?? DateTime.UtcNow;
            
            var (isValid, error) = ValidateCronExpression(cronExpression);
            if (!isValid)
            {
                _logger.LogWarning("Cannot calculate next execution time: invalid cron expression");
                return null;
            }

            // Simplified implementation - for production use a dedicated cron library like NCronTab
            // This returns 1 minute in the future as a placeholder
            var nextTime = baseDateTime.AddMinutes(1);
            _logger.LogDebug("Next execution time calculated: {NextTime} from base: {BaseTime}", nextTime, baseDateTime);
            return nextTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next execution time for cron: {Expression}", cronExpression);
            return null;
        }
    }

    public string GetCronDescription(string cronExpression)
    {
        try
        {
            var description = ExpressionDescriptor.GetDescription(cronExpression);
            _logger.LogDebug("Cron description generated for {Expression}: {Description}", cronExpression, description);
            return description;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate description for cron expression: {Expression}", cronExpression);
            return $"Invalid expression: {ex.Message}";
        }
    }
}

