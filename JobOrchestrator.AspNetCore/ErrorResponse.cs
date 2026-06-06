namespace JobOrchestrator.AspNetCore;

/// <summary>
/// Standard error response format that can be used by exception handlers and middleware.
/// This DTO provides a consistent structure for error responses across the application.
/// </summary>
public class ErrorResponse
{
    /// <summary>The error message describing what went wrong.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>The HTTP status code of the response.</summary>
    public int StatusCode { get; set; }

    /// <summary>The UTC timestamp when the error occurred.</summary>
    public DateTime Timestamp { get; set; }
}
