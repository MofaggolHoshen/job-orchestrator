using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Implement this interface to define what a job does when the scheduler fires it.
/// Register your handler using <c>services.AddJobHandler&lt;THandler&gt;()</c>.
/// Handlers are identified by their fully qualified type name.
/// </summary>
public interface IJobHandler
{
    /// <summary>User-friendly name for this handler, displayed in UI dropdowns (e.g., "Email Job Handler").</summary>
    string Name { get; }

    /// <summary>Called by the scheduler when a due job fires.</summary>
    Task ExecuteAsync(Job job, CancellationToken cancellationToken = default);
}
