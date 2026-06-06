using JobOrchestrator.Core.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JobOrchestrator.Dashboard;

public static class DashboardApplicationBuilderExtensions
{
    /// <summary>
    /// Maps the Scheduler Dashboard Blazor components into the request pipeline and
    /// ensures the scheduler database schema is created on startup.
    /// Call this after <c>services.AddScheduler().WithDashboard()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddScheduler()
    ///     .UseInMemoryStorage()
    ///     .WithDashboard();
    ///
    /// var app = builder.Build();
    /// app.UseSchedulerDashboard();
    /// app.Run();
    /// </code>
    /// </example>
    public static WebApplication UseSchedulerDashboard(this WebApplication app)
    {
        // Initialize DB schema (required for in-memory; no-op for SQL Server with migrations)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();
            db.Database.EnsureCreated();
        }

        app.UseAntiforgery();
        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        return app;
    }
}
