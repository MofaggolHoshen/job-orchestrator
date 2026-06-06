using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Implement this interface to define what a job does when the scheduler fires it.
/// Register your handler using <c>services.AddJobHandler&lt;THandler&gt;(jobType)</c>.
/// </summary>
public interface IJobHandler
{
    /// <summary>The JobType string this handler handles (case-insensitive match).</summary>
    string JobType { get; }

    /// <summary>User-friendly name for this handler, displayed in UI dropdowns (e.g., "Email Job Handler").</summary>
    string Name { get; }

    /// <summary>Called by the scheduler when a due job fires.</summary>
    Task ExecuteAsync(Job job, CancellationToken cancellationToken = default);
}
