using JobOrchestrator.Core.DTOs;
using JobOrchestrator.Core.Models;

namespace JobOrchestrator.Core.Mapping;

/// <summary>
/// Service for mapping between domain models and DTOs.
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// Maps a Job entity to a JobDto.
    /// </summary>
    JobDto MapToJobDto(Job job);

    /// <summary>
    /// Maps multiple Job entities to JobDtos.
    /// </summary>
    IEnumerable<JobDto> MapToJobDtos(IEnumerable<Job> jobs);

    /// <summary>
    /// Maps a JobExecution entity to a JobExecutionDto.
    /// </summary>
    JobExecutionDto MapToJobExecutionDto(JobExecution execution);
}
