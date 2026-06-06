using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JobOrchestrator.Core.Data;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// Only used during dotnet ef commands, not at runtime.
/// </summary>
public class SchedulerDbContextFactory : IDesignTimeDbContextFactory<SchedulerDbContext>
{
    public SchedulerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SchedulerDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=JobOrchestratorDb;Trusted_Connection=true;");
        return new SchedulerDbContext(optionsBuilder.Options);
    }
}
