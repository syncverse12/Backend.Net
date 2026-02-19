using AutoMapper;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Project;
using Graduation_Project.Application.Interfaces;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Graduation_Project.Application.DTOs.Project.Manager;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;

    public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(
    CreateProjectDto dto,
    string managerId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("Invalid project timeline");

        var workspace = await _unitOfWork.Repository<Workspace>()
            .GetByIdAsync(dto.WorkspaceId);

        if (workspace == null || workspace.CreatedByUserId != managerId)
            return Result<ProjectResponseDto>.Failure("Workspace not found or unauthorized");

        var project = _mapper.Map<Project>(dto);
        project.CreatedByUserId = managerId;

        await _unitOfWork.Repository<Project>().AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project created successfully");
    }


    public async Task<Result<ProjectResponseDto>> UpdateAsync(
    string projectId,
    UpdateProjectDto dto,
    string managerId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("Invalid project timeline");

        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .IgnoreQueryFilters()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null || project.IsDeleted)
            return Result<ProjectResponseDto>.Failure("Project not found");

        if (project.Workspace.CreatedByUserId != managerId)
            return Result<ProjectResponseDto>.Failure("Unauthorized");

        _mapper.Map(dto, project);

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project updated successfully");
    }

    public async Task<Result<ProjectResponseDto>> GetByIdAsync(
    string projectId,
    string managerId)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return Result<ProjectResponseDto>.Failure("Project not found");

        if (project.Workspace.CreatedByUserId != managerId)
            return Result<ProjectResponseDto>.Failure("Unauthorized");

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project));
    }

    public async Task<Result<List<ProjectResponseDto>>> GetByWorkspaceAsync(
    string workspaceId,
    string managerId)
    {
        var workspace = await _unitOfWork.Repository<Workspace>()
            .GetByIdAsync(workspaceId);

        if (workspace == null || workspace.CreatedByUserId != managerId)
            return Result<List<ProjectResponseDto>>.Failure("Workspace not found or unauthorized");

        var projects = await _unitOfWork.Repository<Project>()
            .Query()
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync();

        return Result<List<ProjectResponseDto>>.Success(
            _mapper.Map<List<ProjectResponseDto>>(projects));
    }
    public async Task<Result<bool>> DeleteProjectAsync(string projectId, string managerId)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.Milestones) 
            .Include(p => p.Taskitem)      
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return Result<bool>.Failure("Project not found");
        if (project.CreatedByUserId != managerId) return Result<bool>.Failure("Unauthorized");

        project.IsDeleted = true;


        foreach (var milestone in project.Milestones)
        {
            milestone.IsDeleted = true;
        }

        foreach (var task in project.Taskitem)
        {
            task.IsDeleted = true;
        }

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Project and all its components deleted successfully");
    }

    public async Task<Result<bool>> RestoreProjectAsync(string projectId, string managerId)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .IgnoreQueryFilters()
            .Include(p => p.Milestones) 
            .Include(p => p.Taskitem)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return Result<bool>.Failure("Project not found");

        if (project.CreatedByUserId != managerId)
            return Result<bool>.Failure("Unauthorized to restore this project");

        if (!project.IsDeleted)
            return Result<bool>.Failure("Project is already active");

        project.IsDeleted = false;

        foreach (var m in project.Milestones) m.IsDeleted = false;
        foreach (var t in project.Taskitem) t.IsDeleted = false;

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Project restored successfully");
    }

    // --- INVITE EMPLOYEE LOGIC ---
    public async Task<Result<bool>> InviteEmployeeAsync(string projectId, InviteEmployeeDto dto, string managerId)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(projectId);

        if (project == null || project.CreatedByUserId != managerId)
            return Result<bool>.Failure("Project not found or unauthorized.");

        var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == dto.EmployeeId);

        if (isMember)
            return Result<bool>.Failure("User is already a member of this project.");

        var hasPendingInv = await _unitOfWork.Repository<ProjectInvitation>().Query()
            .AnyAsync(i => i.ProjectId == projectId &&
                           i.EmployeeId == dto.EmployeeId &&
                           i.Status == InvitationStatus.Pending);

        if (hasPendingInv)
            return Result<bool>.Failure("An invitation is already pending for this user.");

        var user = await _unitOfWork.Repository<User>()
          .GetByIdAsync(dto.EmployeeId);

        if (user == null || !await _userManager.IsInRoleAsync(user, "Employee"))
            return Result<bool>.Failure("Invalid employee.");

        await _unitOfWork.SaveChangesAsync(); 

        var invitation = new ProjectInvitation
        {
            ProjectId = projectId,
            EmployeeId = dto.EmployeeId,
            SentByManagerId = managerId,
            Type = dto.Type,
            Status = InvitationStatus.Pending,
            SentAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<ProjectInvitation>().AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        var notification = new Notification
        {
            UserId = dto.EmployeeId,
            TriggeredByUserId = managerId,
            Title = $"Invitation to join: {project.Name}",
            Message = $"Project Description: {project.Description ?? "No description provided."} \n Please respond to this invitation.",
            Type = NotificationType.Invitation,
            RelatedEntityId = invitation.Id,
            ActionUrl = $"/employee/invitations/{invitation.Id}",
            IsRead = false
        };
        await _unitOfWork.Repository<Notification>().AddAsync(notification);

        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Invitation sent successfully.");
    }

}