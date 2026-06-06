using Microsoft.EntityFrameworkCore;
using JobOrchestrator.Core.Data;
using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Repositories;

/// <inheritdoc />
public class JobRepository : IJobRepository
{
    private readonly SchedulerDbContext _context;

    public JobRepository(SchedulerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _context.Jobs
            .Include(j => j.Executions.OrderByDescending(e => e.StartTime).Take(10))
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _context.Jobs
            .Include(j => j.Executions.OrderByDescending(e => e.StartTime).Take(5))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetActiveAsync()
    {
        return await _context.Jobs
            .Where(j => j.IsActive)
            .Include(j => j.Executions.OrderByDescending(e => e.StartTime).Take(5))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Job>> GetDueForExecutionAsync(DateTime utcNow)
    {
        return await _context.Jobs
            .Where(j => j.IsActive && j.NextExecutionTime <= utcNow)
            .ToListAsync();
    }

    public async Task AddAsync(Job job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));
        _context.Jobs.Add(job);
        await SaveChangesAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));
        _context.Jobs.Update(job);
        await SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await GetByIdAsync(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Jobs.AnyAsync(j => j.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
