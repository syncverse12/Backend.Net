using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Milestones;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
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

    public async Task<Result<MilestoneResponseDto>> CreateAsync(CreateMilestoneDto dto, string currentUserId)
    {
        var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
            .FirstOrDefaultAsync(m => m.ProjectId == dto.ProjectId && m.UserId == currentUserId);

        if (projectMember == null || projectMember.Role != ProjectRole.ProjectManager)
        {
            return Result<MilestoneResponseDto>.Failure("Unauthorized: Only the Project Manager can create milestones.");
        }

        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(dto.ProjectId);

        if (dto.StartDate < project?.StartDate || dto.EndDate > project?.EndDate)
        {
            return Result<MilestoneResponseDto>.Failure("Milestone dates must be within project start and end dates");
        }

        var milestone = _mapper.Map<Milestone>(dto);
        await _unitOfWork.Repository<Milestone>().AddAsync(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone), "Milestone created successfully");
    }

    public async Task<Result<MilestoneResponseDto>> UpdateAsync(string milestoneId, UpdateMilestoneDto dto, string currentUserId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null) return Result<MilestoneResponseDto>.Failure("Milestone not found");

        var isPM = await _unitOfWork.Repository<ProjectMember>().Query()
            .AnyAsync(m => m.ProjectId == milestone.ProjectId &&
                           m.UserId == currentUserId &&
                           m.Role == ProjectRole.ProjectManager);

        if (!isPM) return Result<MilestoneResponseDto>.Failure("Unauthorized: Only Project Managers can modify milestones.");

        _mapper.Map(dto, milestone);
        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone));
    }

    public async Task<Result<bool>> DeleteAsync(string milestoneId, string currentUserId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null)
            return Result<bool>.Failure("Milestone not found");

        var projectMember = await _unitOfWork.Repository<ProjectMember>()
            .Query()
            .FirstOrDefaultAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId);

        if (projectMember == null || projectMember.Role != ProjectRole.ProjectManager)
        {
            return Result<bool>.Failure("Unauthorized: Only a Project Manager (PM) can delete milestones.");
        }

        milestone.IsDeleted = true;

        var tasks = await _unitOfWork.Repository<TaskItem>()
            .Query()
            .Where(t => t.MilestoneId == milestoneId)
            .ToListAsync();

        foreach (var task in tasks)
        {
            task.IsDeleted = true;
            _unitOfWork.Repository<TaskItem>().Update(task);
        }

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Milestone and its tasks deleted successfully");
    }

    public async Task<Result<MilestoneResponseDto>> GetByIdAsync(string milestoneId, string currentUserId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null)
            return Result<MilestoneResponseDto>.Failure("Milestone not found");

        var isMember = await _unitOfWork.Repository<ProjectMember>()
            .Query()
            .AnyAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId);

        if (!isMember)
            return Result<MilestoneResponseDto>.Failure("Unauthorized: You are not a member of this project.");

        return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone));
    }

    public async Task<Result<List<MilestoneResponseDto>>> GetProjectMilestonesAsync(string projectId, string currentUserId)
    {
        var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);

        if (!isMember) return Result<List<MilestoneResponseDto>>.Failure("Unauthorized: You are not a member of this project.");

        var milestones = await _unitOfWork.Repository<Milestone>()
            .Query()
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.StartDate)
            .ToListAsync();

        return Result<List<MilestoneResponseDto>>.Success(_mapper.Map<List<MilestoneResponseDto>>(milestones));
    }

    public async Task<Result<bool>> RestoreMilestoneAsync(string milestoneId, string currentUserId)
    {
        var milestone = await _unitOfWork.Repository<Milestone>()
            .Query()
            .IgnoreQueryFilters()
            .Include(m => m.Project)
            .Include(m => m.Tasks)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null) return Result<bool>.Failure("Milestone not found");

        var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
            .FirstOrDefaultAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId);

        if (projectMember == null || projectMember.Role != ProjectRole.ProjectManager)
        {
            return Result<bool>.Failure("Unauthorized: Only a PM can restore milestones.");
        }

        if (milestone.Project.IsDeleted)
            return Result<bool>.Failure("Restore the project first.");

        milestone.IsDeleted = false;
        foreach (var task in milestone.Tasks) task.IsDeleted = false;

        _unitOfWork.Repository<Milestone>().Update(milestone);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Milestone restored successfully");
    }
}
