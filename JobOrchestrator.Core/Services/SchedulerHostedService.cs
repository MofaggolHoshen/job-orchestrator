using Microsoft.Extensions.Hosting;

namespace JobOrchestrator.Core.Services;

/// <summary>
/// Hosted service that starts and stops the Job Orchestrator scheduler.
/// This is an internal implementation detail and should not be used directly.
/// </summary>
internal class SchedulerHostedService : BackgroundService
{
    private readonly ISchedulerService _schedulerService;

    /// <summary>
    /// Creates a new instance of the scheduler hosted service.
    /// </summary>
    public SchedulerHostedService(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    /// <summary>
    /// Executes the background scheduler service.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _schedulerService.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    /// <summary>
    /// Stops the background scheduler service.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _schedulerService.StopAsync();
        await base.StopAsync(cancellationToken);
    }
}
