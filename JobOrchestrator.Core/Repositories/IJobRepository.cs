using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Repositories;

/// <summary>
/// Repository for data access operations on Job entities.
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Gets a job by its ID.
    /// </summary>
    Task<Job?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all jobs with their related data (retry policies and recent executions).
    /// </summary>
    Task<IEnumerable<Job>> GetAllAsync();

    /// <summary>
    /// Gets all active jobs with their related data.
    /// </summary>
    Task<IEnumerable<Job>> GetActiveAsync();

    /// <summary>
    /// Gets jobs that are due for execution.
    /// </summary>
    Task<IEnumerable<Job>> GetDueForExecutionAsync(DateTime utcNow);

    /// <summary>
    /// Adds a new job to the repository.
    /// </summary>
    Task AddAsync(Job job);

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    Task UpdateAsync(Job job);

    /// <summary>
    /// Deletes a job by its ID.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks if a job exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
