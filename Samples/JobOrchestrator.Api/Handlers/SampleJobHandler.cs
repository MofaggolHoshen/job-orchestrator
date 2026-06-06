using JobOrchestrator.Core.Models;
using JobOrchestrator.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace JobOrchestrator.Api.Handlers;

/// <summary>
/// Sample job handler for the JobOrchestrator API.
/// Demonstrates how to create a handler that processes scheduled jobs.
/// </summary>
public class SampleJobHandler : IJobHandler
{
    private readonly ILogger<SampleJobHandler> _logger;

    public string Name => "Sample Job Handler";

    public SampleJobHandler(ILogger<SampleJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SampleJobHandler] Job execution started");

        await Task.Delay(100); // Simulate work

        Console.WriteLine("[SampleJobHandler] Job execution completed successfully");
        _logger.LogInformation("[SampleJobHandler] Job execution completed");
    }
}
