using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Milestones;
using SyncVerse.Application.DTOs.Notifications;
using SyncVerse.Application.DTOs.Project;
using SyncVerse.Application.DTOs.Project.Employee;
using SyncVerse.Application.Interfaces.Notifications;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace SyncVerse.Application.Services.Project.Employee
{
    public class EmployeeProjectService : IEmployeeProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public EmployeeProjectService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }
        public async Task<Result<List<ProjectInvitationResponseDto>>> GetMyInvitationsAsync(string employeeId)
        {
            var invitations = await _unitOfWork.Repository<ProjectInvitation>().Query()
                .Include(i => i.Project) 
                .Where(i => i.EmployeeId == employeeId)
                .OrderByDescending(i => i.SentAt)
                .ToListAsync();

            var dtos = invitations.Select(i => new ProjectInvitationResponseDto
            {
                Id = i.Id,
                ProjectId = i.ProjectId,
                ProjectName = i.Project.Name,
                ProjectDescription = i.Project.Description,
                EmployeeId = i.EmployeeId,
                SentByManagerId = i.SentByManagerId,
                Type = i.Type,
                Status = i.Status,
                SentAt = i.SentAt,
                RejectionReason = i.RejectionReason
            }).ToList();

            return Result<List<ProjectInvitationResponseDto>>.Success(dtos);
        }

        public async Task<Result<bool>> RespondToInvitationAsync(string invitationId, RespondInvitationDto dto, string employeeId)
        {
            var invitation = await _unitOfWork.Repository<ProjectInvitation>().GetByIdAsync(invitationId);

            if (invitation == null) return Result<bool>.Failure("Invitation not found.");
            if (invitation.EmployeeId != employeeId) return Result<bool>.Failure("Unauthorized.");
            if (invitation.Status != InvitationStatus.Pending) return Result<bool>.Failure("Already processed.");

            invitation.Status = dto.Status;

            if (dto.Status == InvitationStatus.Accepted)
            {
                var exists = await _unitOfWork.Repository<ProjectMember>().Query()
                    .AnyAsync(m => m.ProjectId == invitation.ProjectId && m.UserId == employeeId);

                if (!exists)
                {
                    var member = new ProjectMember
                    {
                        ProjectId = invitation.ProjectId,
                        UserId = employeeId,
                        Role = invitation.Role, 
                        JoinedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<ProjectMember>().AddAsync(member);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = invitation.SentByManagerId,
                TriggeredByUserId = employeeId,
                Type = NotificationType.System,
                Title = "Invitation Response",
                Message = dto.Status == InvitationStatus.Accepted
                    ? "Employee accepted the invitation."
                    : "Employee rejected the invitation.",
                RelatedEntityId = invitation.Id
            });

            return Result<bool>.Success(true, "Response recorded.");
        }

        public async Task<Result<List<EmployeeProjectResponseDto>>> GetMyProjectsAsync(string employeeId)
        {
            var projects = await _unitOfWork.Repository<Domain.Entities.Project>()
                .Query()
                .Include(p => p.TeamMembers)
                .Include(p => p.Tasks)
                .Where(p => p.TeamMembers.Any(m => m.UserId == employeeId) && !p.IsDeleted)
                .ToListAsync();

            var projectDtos = projects.Select(p =>
            {
                var member = p.TeamMembers.First(m => m.UserId == employeeId);
                var myTasks = p.Tasks.Count(t => t.AssignedUserId == employeeId && !t.IsDeleted);
                var completedTasks = p.Tasks.Count(t => t.AssignedUserId == employeeId && 
                                                        t.Status == TaskStatus.Completed && !t.IsDeleted);

                return new EmployeeProjectResponseDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TotalTasks = p.Tasks.Count(t => !t.IsDeleted),
                    MyTasks = myTasks,
                    CompletedTasks = completedTasks,
                    JoinedAt = member.CreatedAt,
                    RepositoryUrl = p.RepositoryUrl,
                    DocumentationUrl = p.DocumentationUrl
                };
            }).ToList();

            return Result<List<EmployeeProjectResponseDto>>.Success(projectDtos);
        }

        public async Task<Result<EmployeeProjectDetailsDto>> GetProjectDetailsAsync(string projectId, string employeeId)
        {
            var project = await _unitOfWork.Repository<Domain.Entities.Project>()
                .Query()
                .Include(p => p.TeamMembers)
                .Include(p => p.Tasks)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

            if (project == null)
                return Result<EmployeeProjectDetailsDto>.Failure("Project not found");

            var member = project.TeamMembers.FirstOrDefault(m => m.UserId == employeeId);
            if (member == null)
                return Result<EmployeeProjectDetailsDto>.Failure("You are not a member of this project");

            var myTasks = project.Tasks.Count(t => t.AssignedUserId == employeeId && !t.IsDeleted);
            var completedTasks = project.Tasks.Count(t => t.AssignedUserId == employeeId && 
                                                         t.Status == TaskStatus.Completed && !t.IsDeleted);
            var inProgressTasks = project.Tasks.Count(t => t.AssignedUserId == employeeId && 
                                                          t.Status == TaskStatus.InProgress && !t.IsDeleted);

            var milestones = project.Milestones
                .Where(m => !m.IsDeleted)
                .Select(m => new MilestoneResponseDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    IsCompleted = m.IsCompleted,
                    ProjectId = m.ProjectId
                })
                .OrderBy(m => m.StartDate)
                .ToList();

            var details = new EmployeeProjectDetailsDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                TotalTasks = project.Tasks.Count(t => !t.IsDeleted),
                MyTasks = myTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                JoinedAt = member.CreatedAt,
                RepositoryUrl = project.RepositoryUrl,
                DocumentationUrl = project.DocumentationUrl,
                Milestones = milestones
            };

            return Result<EmployeeProjectDetailsDto>.Success(details);
        }
    }
}