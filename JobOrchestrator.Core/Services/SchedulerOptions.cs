namespace JobOrchestrator.Core.Services;

/// <summary>
/// Specifies the storage backend for the Job Orchestrator.
/// </summary>
public enum SchedulerStorageType
{
    /// <summary>In-memory storage (data lost on application restart).</summary>
    InMemory,
    
    /// <summary>SQL Server storage (persistent data).</summary>
    SqlServer
}

/// <summary>
/// Configuration options for the Job Orchestrator scheduler.
/// </summary>
public class SchedulerOptions
{
    /// <summary>
    /// The type of storage backend to use (InMemory or SqlServer).
    /// </summary>
    public SchedulerStorageType StorageType { get; set; } = SchedulerStorageType.InMemory;

    /// <summary>
    /// The connection string for SQL Server storage.
    /// Required when StorageType is SqlServer.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Name used for the in-memory database. Defaults to a shared instance so data persists
    /// for the lifetime of the application.
    /// </summary>
    public string InMemoryDatabaseName { get; set; } = "SchedulerDb";

    /// <summary>
    /// How often (in seconds) the scheduler polls for due jobs.
    /// Lower values (1-5 seconds) for responsive scheduling; higher values (30+ seconds) for lower CPU usage.
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of jobs that can run in parallel.
    /// Balance between throughput and system resources.
    /// </summary>
    public int MaxParallelJobs { get; set; } = 5;
}
