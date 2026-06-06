using Microsoft.EntityFrameworkCore;
using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Data;

public class SchedulerDbContext : DbContext
{
    public SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<JobExecution> JobExecutions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HandlerType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.HandlerName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CronExpression).IsRequired().HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxRetries).HasDefaultValue(3);
            entity.Property(e => e.RetryIntervalSeconds).HasDefaultValue(60);
            entity.Property(e => e.BackoffMultiplier).HasDefaultValue(1.5m).HasPrecision(5, 2);
            entity.HasMany(e => e.Executions).WithOne(ex => ex.Job).HasForeignKey(ex => ex.JobId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
        });
    }
}
