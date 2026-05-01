using AutoMapper;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Notifications;
using SyncVerse.Application.DTOs.Project;
using SyncVerse.Application.DTOs.Project.Manager;
using SyncVerse.Application.Interfaces;
using SyncVerse.Application.Interfaces.Notifications;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly INotificationService _notificationService;

    public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectDto dto, string currentUserId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("End date cannot be earlier than start date.");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null || string.IsNullOrEmpty(user.WorkspaceId))
            return Result<ProjectResponseDto>.Failure("User or Workspace not found.");

        var workspace = await _unitOfWork.Repository<Workspace>().GetByIdAsync(user.WorkspaceId);
        if (workspace == null)
            return Result<ProjectResponseDto>.Failure("Workspace not found.");

        var isWorkspaceOwner = workspace.CreatedByUserId == currentUserId;

        var isManagerRole = await _userManager.IsInRoleAsync(user, "ProjectManager") || 
                            await _userManager.IsInRoleAsync(user, "Manager") || 
                            await _userManager.IsInRoleAsync(user, "Admin");

        if (!isWorkspaceOwner && !isManagerRole)
        {
            return Result<ProjectResponseDto>.Failure("Unauthorized: Only Workspace Owners, Managers, or Project Managers can create projects.");
        }

        var project = _mapper.Map<Project>(dto);
        project.WorkspaceId = user.WorkspaceId;
        project.CreatedByUserId = currentUserId;
        project.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Repository<Project>().AddAsync(project);

        var adminMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = currentUserId,
            Role = ProjectRole.ProjectManager,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ProjectMember>().AddAsync(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project created successfully.");
    }


    public async Task<Result<ProjectResponseDto>> UpdateAsync(string projectId, UpdateProjectDto dto, string currentUserId)
    {
        if (dto.EndDate < dto.StartDate)
            return Result<ProjectResponseDto>.Failure("Invalid project timeline");

        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project?.Workspace == null)
            return Result<ProjectResponseDto>.Failure("Project workspace data is missing.");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null || user.WorkspaceId != project.WorkspaceId)
            return Result<ProjectResponseDto>.Failure("Unauthorized: You do not belong to the project's workspace.");

        var isAuthorized = project.Workspace!.CreatedByUserId == currentUserId ||
                           await _unitOfWork.Repository<ProjectMember>().Query()
                               .AnyAsync(m => m.ProjectId == projectId &&
                                              m.UserId == currentUserId &&
                                              m.Role == ProjectRole.ProjectManager);

        if (!isAuthorized)
            return Result<ProjectResponseDto>.Failure("Unauthorized: Only Workspace Owner or Project Manager can update project details.");

        _mapper.Map(dto, project);

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project),
            "Project updated successfully");
    }

    public async Task<Result<ProjectResponseDto>> GetByIdAsync(string projectId, string currentUserId)
    {
        var project = await _unitOfWork.Repository<Project>()
            .Query()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
            return Result<ProjectResponseDto>.Failure("Project not found");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null || user.WorkspaceId != project.WorkspaceId)
            return Result<ProjectResponseDto>.Failure("Unauthorized: You do not belong to the project's workspace.");

        var isAuthorized = project.Workspace?.CreatedByUserId == currentUserId ||
                           await _unitOfWork.Repository<ProjectMember>().Query()
                               .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);

        if (!isAuthorized)
            return Result<ProjectResponseDto>.Failure("Unauthorized: You don't have permission to view this project.");

        return Result<ProjectResponseDto>.Success(
            _mapper.Map<ProjectResponseDto>(project));
    }

    public async Task<Result<List<ProjectResponseDto>>> GetByWorkspaceForManagerAsync(string workspaceId,string managerId)
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

    public async Task<Result<bool>> DeleteProjectAsync(string projectId, string currentUserId)
    {
        var project = await _unitOfWork.Repository<Project>() 
            .Query()
            .Include(p => p.Workspace) 
            .Include(p => p.Milestones)
            .Include(p => p.Taskitem)
            .Include(p => p.TeamMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null) return Result<bool>.Failure("Project not found");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null || user.WorkspaceId != project.WorkspaceId)
            return Result<bool>.Failure("Unauthorized: You do not belong to the project's workspace.");

        var isProjectManager = project.TeamMembers
            .Any(pm => pm.UserId == currentUserId && pm.Role == ProjectRole.ProjectManager && pm.IsActive);

        var isAuthorized = project.Workspace?.CreatedByUserId == currentUserId ||
                           project.CreatedByUserId == currentUserId || 
                           isProjectManager;

        if (!isAuthorized)
            return Result<bool>.Failure("Unauthorized: Only the Workspace Owner, Project Creator, or Project Manager can delete this project.");

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

        return Result<bool>.Success(true, "Project, milestones, and tasks deleted successfully");
    }

    public async Task<Result<bool>> RestoreProjectAsync(string projectId, string currentUserId)
    {
        var project = await _unitOfWork.Repository<Project>() 
            .Query()
            .IgnoreQueryFilters() 
            .Include(p => p.Workspace) 
            .Include(p => p.Milestones)
            .Include(p => p.Taskitem)
            .Include(p => p.TeamMembers)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return Result<bool>.Failure("Project not found");

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user == null || user.WorkspaceId != project.WorkspaceId)
            return Result<bool>.Failure("Unauthorized: You do not belong to the project's workspace.");

        var isProjectManager = project.TeamMembers
            .Any(pm => pm.UserId == currentUserId && pm.Role == ProjectRole.ProjectManager);

        var isAuthorized = project.Workspace?.CreatedByUserId == currentUserId ||
                           project.CreatedByUserId == currentUserId ||
                           isProjectManager;

        if (!isAuthorized)
            return Result<bool>.Failure("Unauthorized to restore this project");

        if (!project.IsDeleted)
            return Result<bool>.Failure("Project is already active");

        project.IsDeleted = false;

        foreach (var m in project.Milestones) m.IsDeleted = false;
        foreach (var t in project.Taskitem) t.IsDeleted = false;

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Project and its components restored successfully");
    }

    // --- INVITE EMPLOYEE LOGIC ---
    public async Task<Result<bool>> InviteEmployeeAsync(string projectId, InviteEmployeeDto dto, string currentUserId)
    {
        var project = await _unitOfWork.Repository<Project>().Query()
            .Include(p => p.Workspace)
            .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

        if (project == null)
            return Result<bool>.Failure("Project not found.");

        var isAuthorized = project.Workspace?.CreatedByUserId == currentUserId ||
                           project.CreatedByUserId == currentUserId;

        if (!isAuthorized)
            return Result<bool>.Failure("Unauthorized: Only the Workspace Owner or Project Creator can invite employees.");

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

        var user = await _userManager.FindByIdAsync(dto.EmployeeId);
        if (user == null) return Result<bool>.Failure("Employee not found.");

        var sentAt = DateTime.UtcNow;
        var invitation = new ProjectInvitation
        {
            ProjectId = projectId,
            EmployeeId = dto.EmployeeId,
            SentByManagerId = currentUserId,
            Type = dto.Type,
            Role = dto.Role,
            Status = InvitationStatus.Pending,
            SentAt = sentAt,
            ExpiresAt = sentAt.AddDays(3)
        };

        await _unitOfWork.Repository<ProjectInvitation>().AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = dto.EmployeeId,
            TriggeredByUserId = currentUserId,
            Type = NotificationType.Invitation,
            Title = $"Invitation to join: {project.Name}",
            Message = $"You have been invited to join as {dto.Role} in project: {project.Name}.",
            RelatedEntityId = invitation.Id,
            ActionUrl = $"/employee/invitations/{invitation.Id}"
        });

        return Result<bool>.Success(true, "Invitation sent successfully.");
    }

    public async Task<List<string>> GetAcceptedEmployeeNamesAsync(string projectId)
    {
        var acceptedInvitations = await _unitOfWork.Repository<ProjectInvitation>()
            .Query()
            .Where(inv => inv.ProjectId == projectId && inv.Status == InvitationStatus.Accepted)
            .ToListAsync();

        var employeeIds = acceptedInvitations.Select(inv => inv.EmployeeId).Distinct().ToList();
        var users = await _userManager.Users.Where(u => employeeIds.Contains(u.Id)).ToListAsync();
        return users.Select(u => u.FirstName + " " + u.LastName).ToList();
    }
}