using AutoMapper;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Milestones;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using Microsoft.EntityFrameworkCore;


using ProjectEntity = SyncVerse.Domain.Entities.Project;

namespace SyncVerse.Application.Services.Milestones
{
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
            var project = await _unitOfWork.Repository<ProjectEntity>().Query()
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null) return Result<MilestoneResponseDto>.Failure("Project not found");

            var isWorkspaceOwner = project.Workspace?.CreatedByUserId == currentUserId;
            var isPM = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(m => m.ProjectId == dto.ProjectId && m.UserId == currentUserId && m.Role == ProjectRole.ProjectManager);

            if (!isWorkspaceOwner && !isPM)
                return Result<MilestoneResponseDto>.Failure("Unauthorized: Only the Workspace Owner or Project Manager can create milestones.");

            if (dto.StartDate < project.StartDate || dto.EndDate > project.EndDate)
                return Result<MilestoneResponseDto>.Failure("Milestone dates must be within project bounds.");

            var milestone = _mapper.Map<Milestone>(dto);
            await _unitOfWork.Repository<Milestone>().AddAsync(milestone);
            await _unitOfWork.SaveChangesAsync();

            return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone), "Milestone created successfully");
        }

        public async Task<Result<MilestoneResponseDto>> UpdateAsync(string milestoneId, UpdateMilestoneDto dto, string currentUserId)
        {
            var milestone = await _unitOfWork.Repository<Milestone>().Query()
                .Include(m => m.Project).ThenInclude(p => p.Workspace)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null) return Result<MilestoneResponseDto>.Failure("Milestone not found");

            var isWorkspaceOwner = milestone.Project.Workspace?.CreatedByUserId == currentUserId;
            var isPM = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(m => m.ProjectId == milestone.ProjectId && m.UserId == currentUserId && m.Role == ProjectRole.ProjectManager);

            if (!isWorkspaceOwner && !isPM)
                return Result<MilestoneResponseDto>.Failure("Unauthorized: Only Workspace Owners or PMs can modify milestones.");

            _mapper.Map(dto, milestone);
            _unitOfWork.Repository<Milestone>().Update(milestone);
            await _unitOfWork.SaveChangesAsync();

            return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone));
        }

        public async Task<Result<bool>> DeleteAsync(string milestoneId, string currentUserId)
        {
            var milestone = await _unitOfWork.Repository<Milestone>().Query()
                .Include(m => m.Project).ThenInclude(p => p.Workspace)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null) return Result<bool>.Failure("Milestone not found");

            var isWorkspaceOwner = milestone.Project.Workspace?.CreatedByUserId == currentUserId;
            var isPM = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId && pm.Role == ProjectRole.ProjectManager);

            if (!isWorkspaceOwner && !isPM)
                return Result<bool>.Failure("Unauthorized: Only Workspace Owners or PMs can delete milestones.");

            milestone.IsDeleted = true;

            var tasks = await _unitOfWork.Repository<TaskItem>().Query()
                .Where(t => t.MilestoneId == milestoneId)
                .ToListAsync();

            foreach (var task in tasks) { task.IsDeleted = true; }

            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Success(true, "Milestone and its tasks deleted.");
        }

        public async Task<Result<MilestoneResponseDto>> GetByIdAsync(string milestoneId, string currentUserId)
        {
            var milestone = await _unitOfWork.Repository<Milestone>().Query()
                .Include(m => m.Project).ThenInclude(p => p.Workspace)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null) return Result<MilestoneResponseDto>.Failure("Milestone not found");

            var isWorkspaceOwner = milestone.Project.Workspace?.CreatedByUserId == currentUserId;
            var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId);

            if (!isWorkspaceOwner && !isMember)
                return Result<MilestoneResponseDto>.Failure("Unauthorized.");

            return Result<MilestoneResponseDto>.Success(_mapper.Map<MilestoneResponseDto>(milestone));
        }

        public async Task<Result<List<MilestoneResponseDto>>> GetProjectMilestonesAsync(string projectId, string currentUserId)
        {
            var project = await _unitOfWork.Repository<ProjectEntity>().Query()
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return Result<List<MilestoneResponseDto>>.Failure("Project not found");

            var isWorkspaceOwner = project.Workspace?.CreatedByUserId == currentUserId;
            var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);

            if (!isWorkspaceOwner && !isMember)
                return Result<List<MilestoneResponseDto>>.Failure("Unauthorized.");

            var milestones = await _unitOfWork.Repository<Milestone>().Query()
                .Where(m => m.ProjectId == projectId && !m.IsDeleted)
                .OrderBy(m => m.StartDate)
                .ToListAsync();

            return Result<List<MilestoneResponseDto>>.Success(_mapper.Map<List<MilestoneResponseDto>>(milestones));
        }

        public async Task<Result<bool>> RestoreMilestoneAsync(string milestoneId, string currentUserId)
        {
            var milestone = await _unitOfWork.Repository<Milestone>().Query()
                .IgnoreQueryFilters()
                .Include(m => m.Project).ThenInclude(p => p.Workspace)
                .Include(m => m.Tasks)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone == null) return Result<bool>.Failure("Milestone not found");

            var isWorkspaceOwner = milestone.Project.Workspace?.CreatedByUserId == currentUserId;
            var isPM = await _unitOfWork.Repository<ProjectMember>().Query()
                .AnyAsync(pm => pm.ProjectId == milestone.ProjectId && pm.UserId == currentUserId && pm.Role == ProjectRole.ProjectManager);

            if (!isWorkspaceOwner && !isPM)
                return Result<bool>.Failure("Unauthorized.");

            if (milestone.Project.IsDeleted)
                return Result<bool>.Failure("Restore the project first.");

            milestone.IsDeleted = false;
            foreach (var task in milestone.Tasks) task.IsDeleted = false;

            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Success(true, "Milestone restored.");
        }
    }
}
