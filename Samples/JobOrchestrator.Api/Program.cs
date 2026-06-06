using JobOrchestrator.Api.Handlers;
using JobOrchestrator.Core.Services;
using JobOrchestrator.AspNetCore.Middleware;
using JobOrchestrator.Core.Extensions;
using JobOrchestrator.Dashboard;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        // Add services to the container
        builder.Services
            .AddScheduler()
            .WithDashboard()
            .UseInMemoryStorage()
            .AddJobHandler<SampleJobHandler>()
            .Configure(options =>
            {
                options.PollingIntervalSeconds = 5;
                options.MaxParallelJobs = 3;
            });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Job Orchestrator API",
                Version = "v1",
                Description = "API for managing and scheduling background jobs"
            });
        });

        var app = builder.Build();

        // Add global exception handling middleware
        app.UseJobOrchestratorExceptionHandling();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Orchestrator API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Use dashboard before mapping controllers
        app.UseSchedulerDashboard();
        app.MapControllers();

        // Simple handler endpoint that prints something
        app.MapGet("/api/handler", () =>
            {
                app.Logger.LogInformation("Handler endpoint called");
                return Results.Ok(new
                {
                    message = "Handler executed successfully",
                    timestamp = DateTime.UtcNow
                });
            })
            .WithName("SimpleHandler")
            .WithOpenApi();

        app.Run();
    }
}