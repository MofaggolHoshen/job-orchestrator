using JobOrchestrator.Core.DTOs;
using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Mapping;

/// <inheritdoc />
public class MappingService : IMappingService
{
    public JobDto MapToJobDto(Job job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));

        return new JobDto
        {
            Id = job.Id,
            Name = job.Name,
            HandlerType = job.HandlerType,
            HandlerName = job.HandlerName,
            CronExpression = job.CronExpression,
            IsActive = job.IsActive,
            NextExecutionTime = job.NextExecutionTime,
            LastExecutedAt = job.LastExecutedAt,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            RetryPolicy = new RetryPolicyDto
            {
                MaxRetries = job.MaxRetries,
                RetryIntervalSeconds = job.RetryIntervalSeconds,
                BackoffMultiplier = job.BackoffMultiplier
            },
            RecentExecutions = job.Executions.Select(MapToJobExecutionDto).ToList()
        };
    }

    public IEnumerable<JobDto> MapToJobDtos(IEnumerable<Job> jobs)
    {
        ArgumentNullException.ThrowIfNull(jobs, nameof(jobs));
        return jobs.Select(MapToJobDto);
    }

    public JobExecutionDto MapToJobExecutionDto(JobExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution, nameof(execution));

        return new JobExecutionDto
        {
            Id = execution.Id,
            StartTime = execution.StartTime,
            EndTime = execution.EndTime,
            Status = execution.Status.ToString(),
            ErrorMessage = execution.ErrorMessage,
            ExecutionAttempt = execution.ExecutionAttempt
        };
    }
}
