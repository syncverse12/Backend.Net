using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Milestones;

public interface IMilestoneService
{
    Task<Result<MilestoneResponseDto>> CreateAsync(CreateMilestoneDto dto, string managerId);
    Task<Result<MilestoneResponseDto>> UpdateAsync(string milestoneId, UpdateMilestoneDto dto, string managerId);
    Task<Result<bool>> DeleteAsync(string milestoneId, string managerId);
    Task<Result<MilestoneResponseDto>> GetByIdAsync(string milestoneId, string managerId);
    Task<Result<List<MilestoneResponseDto>>> GetProjectMilestonesAsync(string projectId, string managerId);
}
