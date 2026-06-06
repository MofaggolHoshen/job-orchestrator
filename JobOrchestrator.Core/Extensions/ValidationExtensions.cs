using JobOrchestrator.Core.Constants;
using JobOrchestrator.Core.Exceptions;

namespace JobOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for common validation operations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that a string is not null, empty, or whitespace.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the string is null, empty, or whitespace.</exception>
    public static string ValidateNotNullOrEmpty(this string? value, string parameterName, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(errorMessage, parameterName);
        }

        return value;
    }

    /// <summary>
    /// Validates that a numeric value is positive.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is less than or equal to 0.</exception>
    public static T ValidatePositive<T>(this T value, string parameterName, string errorMessage)
        where T : IComparable<T>
    {
        if (value.CompareTo(default(T)) <= 0)
        {
            throw new ArgumentException(errorMessage, parameterName);
        }

        return value;
    }

    /// <summary>
    /// Validates that a numeric value is non-negative.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is negative.</exception>
    public static T ValidateNonNegative<T>(this T value, string parameterName, string errorMessage)
        where T : IComparable<T>
    {
        if (value.CompareTo(default(T)) < 0)
        {
            throw new ArgumentException(errorMessage, parameterName);
        }

        return value;
    }
}
