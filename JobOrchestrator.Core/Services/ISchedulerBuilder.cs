using Microsoft.Extensions.DependencyInjection;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Fluent builder returned by <c>services.AddScheduler()</c>.
/// Chain storage configuration, job handlers, and optional features (Dashboard, etc.)
/// to get compile-time IntelliSense for all scheduler-related registrations.
/// </summary>
/// <example>
/// <code>
/// builder.Services
///     .AddScheduler()
///     .UseInMemoryStorage()
///     .Configure(opt => opt.PollingIntervalSeconds = 10)
///     .AddJobHandler&lt;MyJobHandler&gt;()
///     .WithDashboard();   // requires BulkMessage.Orchestrator.Dashboard reference
/// </code>
/// </example>
public interface ISchedulerBuilder
{
    /// <summary>The underlying service collection — use for advanced registrations.</summary>
    IServiceCollection Services { get; }
}

/// <inheritdoc />
public sealed class SchedulerBuilder : ISchedulerBuilder
{
    public IServiceCollection Services { get; }

    internal SchedulerBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
