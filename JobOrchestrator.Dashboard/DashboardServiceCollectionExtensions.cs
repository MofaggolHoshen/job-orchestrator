using JobOrchestrator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobOrchestrator.Dashboard;

public static class DashboardServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Scheduler Dashboard Blazor UI to the service collection.
    /// After calling this, map the dashboard routes in your pipeline with
    /// <c>app.UseSchedulerDashboard()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddScheduler()
    ///     .UseInMemoryStorage()
    ///     .AddJobHandler&lt;MyHandler&gt;()
    ///     .WithDashboard();
    ///
    /// // ...
    /// app.UseSchedulerDashboard();
    /// </code>
    /// </example>
    public static ISchedulerBuilder WithDashboard(this ISchedulerBuilder builder)
    {
        builder.Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        return builder;
    }
}
