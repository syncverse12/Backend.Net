using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Milestones;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class MilestoneService : IMilestoneService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MilestoneService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MilestoneResponseDto>> CreateAsync(CreateMilestoneDto dto, string managerId)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(dto.ProjectId);
        if (project == null || project.CreatedByUserId != managerId)
            return Result<MilestoneResponseDto>.Failure("Project not found or unauthorized");

        if (dto.StartDate < project.StartDate || dto.EndDate > project.EndDate)
        {
            return Result<MilestoneResponseDto>.Failure("Milestone dates must be within project start and end dates");
        }

        var milestone = _mapper.Map<Milestone>(dto);

        await _unitOfWork.Repository<Milestone>().AddAsync(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone), "Milestone created successfully");
    }

    public async Task<Result<MilestoneResponseDto>> UpdateAsync(string milestoneId, UpdateMilestoneDto dto, string managerId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null)
            return Result<MilestoneResponseDto>.Failure("Milestone not found");

        if (milestone.Project.CreatedByUserId != managerId)
            return Result<MilestoneResponseDto>.Failure("Unauthorized");

        _mapper.Map(dto, milestone);

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone), "Milestone updated successfully");
    }

    public async Task<Result<bool>> DeleteAsync(string milestoneId, string managerId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null)
            return Result<bool>.Failure("Milestone not found");

        if (milestone.Project.CreatedByUserId != managerId)
            return Result<bool>.Failure("Unauthorized");

        milestone.IsDeleted = true;
        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Milestone deleted successfully");
    }

    public async Task<Result<MilestoneResponseDto>> GetByIdAsync(string milestoneId, string managerId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId && m.Project.CreatedByUserId == managerId);

        if (milestone == null)
            return Result<MilestoneResponseDto>.Failure("Milestone not found");

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone));
    }

    public async Task<Result<List<MilestoneResponseDto>>> GetProjectMilestonesAsync(string projectId, string managerId)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(projectId);
        if (project == null || project.CreatedByUserId != managerId)
            return Result<List<MilestoneResponseDto>>.Failure("Project not found or unauthorized");

        var milestones = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();

        return Result<List<MilestoneResponseDto>>.Success(_mapper.Map<List<MilestoneResponseDto>>(milestones));
    }

    public async Task<Result<bool>> RestoreMilestoneAsync(string milestoneId, string managerId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .IgnoreQueryFilters() 
            .Include(m => m.Project) 
            .Include(m => m.Tasks)   
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null) return Result<bool>.Failure("Milestone not found");

        if (milestone.Project.IsDeleted)
            return Result<bool>.Failure("Cannot restore milestone because the parent project is deleted. Restore the project first.");

        milestone.IsDeleted = false;
        foreach (var task in milestone.Tasks)
        {
            task.IsDeleted = false;
        }

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Milestone and its tasks restored successfully");
    }
}
