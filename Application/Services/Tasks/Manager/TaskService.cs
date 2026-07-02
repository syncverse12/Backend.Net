using AutoMapper;
using SyncVerse.Application.Common.Pagination;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Notifications;
using SyncVerse.Application.DTOs.Tasks;
using SyncVerse.Application.DTOs.Tasks.Manager;
using SyncVerse.Application.Interfaces.Notifications;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Task.Manager;
using SyncVerse.Domain.Entities;
using SyncVerse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.AspNetCore.Identity;

using ProjectEntity = SyncVerse.Domain.Entities.Project;

namespace SyncVerse.Application.Services.Task.Manager
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        //CREATE
        public async Task<Result<TaskResponseDto>> CreateAsync(CreateTaskDto dto, string currentUserId)
        {
            var project = await _unitOfWork.Repository<ProjectEntity>().Query()
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);

            if (project == null)
                return Result<TaskResponseDto>.Failure("Project not found");

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null || user.WorkspaceId != project.WorkspaceId)
                return Result<TaskResponseDto>.Failure("Unauthorized: You do not belong to the project's workspace.");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == dto.ProjectId && m.UserId == currentUserId);

            // ✅ Check permissions: Workspace Owner OR Project Creator OR ProjectMember(ProjectManager) OR Team Leader
            bool isWorkspaceOwner = project.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectCreator = project.CreatedByUserId == currentUserId;
            bool isProjectManagerMember = projectMember?.Role == ProjectRole.ProjectManager;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectCreator && !isProjectManagerMember && !isTeamLeader)
            {
                return Result<TaskResponseDto>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, Project Creator, or Team Leader can create tasks.");
            }

            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await _unitOfWork.Repository<TaskCategory>().Query()
                    .AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

                if (!categoryExists)
                    return Result<TaskResponseDto>.Failure("The selected Category is invalid or has been deleted.");
            }

            var milestone = await _unitOfWork.Repository<Milestone>().GetByIdAsync(dto.MilestoneId);
            if (milestone == null) return Result<TaskResponseDto>.Failure("Milestone not found");

            if (dto.DueDate.HasValue && (dto.DueDate.Value < milestone.StartDate || dto.DueDate.Value > milestone.EndDate))
            {
                return Result<TaskResponseDto>.Failure($"Task Due Date must be between {milestone.StartDate:yyyy-MM-dd} and {milestone.EndDate:yyyy-MM-dd}");
            }

            if (!string.IsNullOrEmpty(dto.AssignedToUserId))
            {
                var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
                    .AnyAsync(m => m.ProjectId == dto.ProjectId
                                && m.UserId == dto.AssignedToUserId);

                if (!isMember)
                    return Result<TaskResponseDto>.Failure("Assigned user is not a member of this project.");
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = TaskStatus.Pending,
                CreatedByUserId = currentUserId,
                AssignedToUserId = dto.AssignedToUserId,
                ProjectId = dto.ProjectId,
                MilestoneId = dto.MilestoneId,
                CategoryId = dto.CategoryId,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TaskItem>().AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            var taskWithDetails = await _unitOfWork.Repository<TaskItem>().Query()
               .Include(t => t.Category)
               .Include(t => t.AssignedToUser)
               .Include(t => t.CreatedByUser)
               .FirstOrDefaultAsync(t => t.Id == task.Id);

            if (!string.IsNullOrEmpty(dto.AssignedToUserId))
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = dto.AssignedToUserId,
                    TriggeredByUserId = currentUserId,
                    Title = "New Task Assigned",
                    Message = $"You have been assigned a new task: {task.Title}",
                    Type = NotificationType.TaskAssigned,
                    RelatedEntityId = task.Id
                });
            }

            return Result<TaskResponseDto>.Success(_mapper.Map<TaskResponseDto>(taskWithDetails ?? task), "Task Created Successfully");
        }


        //GET
        public async Task<Result<PagedResult<TaskResponseDto>>> GetManagerTasksAsync(string managerId,TaskQuery query)
        {
            var managerProjectIds = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .Where(pm => pm.UserId == managerId &&
                             (pm.Role == ProjectRole.ProjectManager || pm.Role == ProjectRole.TeamLeader))
                .Select(pm => pm.ProjectId)
                .Where(id => id != null)
                .ToListAsync();

            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .Where(t => t.ProjectId != null && managerProjectIds.Contains(t.ProjectId!));

            // Filters
            if (query.Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);

            if (query.Priority.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority.Value);

            if (query.IsDeleted.HasValue)
                tasksQuery = tasksQuery.Where(t => t.IsDeleted == query.IsDeleted.Value);

            if (!string.IsNullOrWhiteSpace(query.CategoryId))
            {
                tasksQuery = tasksQuery.Where(t => t.CategoryId == query.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(query.Search));

            tasksQuery = query.SortBy switch
            {
                TaskSortBy.Oldest => tasksQuery.OrderBy(t => t.CreatedAt),
                TaskSortBy.Title => tasksQuery.OrderBy(t => t.Title),
                TaskSortBy.Priority => tasksQuery.OrderByDescending(t => t.Priority),
                _ => tasksQuery.OrderByDescending(t => t.CreatedAt),
            };

            var totalCount = await tasksQuery.CountAsync();

            var tasks = await tasksQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return Result<PagedResult<TaskResponseDto>>.Success(
                new PagedResult<TaskResponseDto>
                {
                    Items = _mapper.Map<List<TaskResponseDto>>(tasks),
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize
                });
        }


        //UPDATE
        public async Task<Result<TaskResponseDto>> UpdateAsync(string taskId, UpdateTaskDto dto, string currentUserId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Milestone)
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return Result<TaskResponseDto>.Failure("Task not found");

            if (task.Status == TaskStatus.Completed)
                return Result<TaskResponseDto>.Failure("Cannot reassign or modify a completed task.");

            if (task.IsDeleted)
                return Result<TaskResponseDto>.Failure("Cannot modify or assign a deleted task.");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()   
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == currentUserId);

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null || user.WorkspaceId != task.Project?.WorkspaceId)
                return Result<TaskResponseDto>.Failure("Unauthorized: You do not belong to the project's workspace.");

            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectManager = task.Project?.CreatedByUserId == currentUserId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<TaskResponseDto>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can modify tasks.");
            }

            if (!string.IsNullOrEmpty(dto.AssignedToUserId))
            {
                var isMember = await _unitOfWork.Repository<ProjectMember>().Query()
                    .AnyAsync(m => m.ProjectId == task.ProjectId && m.UserId == dto.AssignedToUserId);

                if (!isMember)
                    return Result<TaskResponseDto>.Failure("Assigned user is not a member of this project.");
            }

            if (task.Status == TaskStatus.Submitted)
            {
                return Result<TaskResponseDto>.Failure("Task is submitted and cannot be modified. Please Review first.");
            }

            if (dto.DueDate.HasValue && task.Milestone != null)
            {
                if (dto.DueDate.Value < task.Milestone.StartDate || dto.DueDate.Value > task.Milestone.EndDate)
                {
                    return Result<TaskResponseDto>.Failure("New Due Date is outside Milestone bounds.");
                }
            }

            if (!string.IsNullOrEmpty(dto.CategoryId))
            {
                var categoryExists = await _unitOfWork.Repository<TaskCategory>().Query()
                    .AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);

                if (!categoryExists)
                    return Result<TaskResponseDto>.Failure("The selected Category is invalid.");
            }

            if (!IsValidStatusTransition(task.Status, dto.Status))
            {
                return Result<TaskResponseDto>.Failure(
                    $"Invalid status transition from '{task.Status}' to '{dto.Status}'. " +
                    "Please follow the correct workflow: Pending → InProgress → Submitted → Completed/Rejected");
            }

            if (task.Status != TaskStatus.InProgress && dto.Status == TaskStatus.InProgress)
            {
                task.TaskStartedAt = DateTime.UtcNow; 
            }

            if (task.Status != TaskStatus.Completed && dto.Status == TaskStatus.Completed)
            {
                task.TaskCompletedAt = DateTime.UtcNow; 
            }

            var oldAssignedUserId = task.AssignedToUserId;
            var isReassignment = oldAssignedUserId != dto.AssignedToUserId;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.CategoryId = dto.CategoryId;
            task.AssignedToUserId = dto.AssignedToUserId;
            task.DueDate = dto.DueDate;
            task.Status = dto.Status;

            _unitOfWork.Repository<TaskItem>().Update(task);

            await _unitOfWork.SaveChangesAsync();

            if (isReassignment)
            {
                if (!string.IsNullOrEmpty(oldAssignedUserId))
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = oldAssignedUserId,
                        TriggeredByUserId = currentUserId,
                        Title = "Task Unassigned",
                        Message = $"You have been unassigned from task: {task.Title}",
                        Type = NotificationType.System,
                        RelatedEntityId = task.Id
                    });
                }

                if (!string.IsNullOrEmpty(dto.AssignedToUserId))
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = dto.AssignedToUserId,
                        TriggeredByUserId = currentUserId,
                        Title = "New Task Assigned",
                        Message = $"You have been assigned a new task: {task.Title}",
                        Type = NotificationType.TaskAssigned,
                        RelatedEntityId = task.Id
                    });
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(task.AssignedToUserId))
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = task.AssignedToUserId,
                        TriggeredByUserId = currentUserId,
                        Title = "Task Updated",
                        Message = $"Details for task '{task.Title}' have been updated by the manager.",
                        Type = NotificationType.System,
                        RelatedEntityId = task.Id
                    });
                }
            }

            var updatedTask = await _unitOfWork.Repository<TaskItem>()
                  .Query()
                  .Include(t => t.Category)
                  .Include(t => t.AssignedToUser)
                  .Include(t => t.CreatedByUser)
                  .FirstOrDefaultAsync(t => t.Id == taskId);

            return Result<TaskResponseDto>.Success(
                _mapper.Map<TaskResponseDto>(updatedTask),
                "Task updated successfully"
            );
        }


        //DELETE
        public async Task<Result<bool>> DeleteAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().Query()
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return Result<bool>.Failure("Task not found");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.WorkspaceId != task.Project?.WorkspaceId)
                return Result<bool>.Failure("Unauthorized: You do not belong to the project's workspace.");

            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == userId;
            bool isProjectManager = task.Project?.CreatedByUserId == userId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<bool>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can delete tasks.");
            }

            var dependencies = await _unitOfWork.Repository<TaskDependency>().Query()
                .Where(d => d.TaskId == taskId || d.DependsOnTaskId == taskId)
                .ToListAsync();

            foreach (var dep in dependencies)
            {
                _unitOfWork.Repository<TaskDependency>().Delete(dep);
            }

            task.IsDeleted = true;
            _unitOfWork.Repository<TaskItem>().Update(task);

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task deleted successfully");
        }

        //RESTORE
        public async Task<Result<bool>> RestoreAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .IgnoreQueryFilters()
                .Include(t => t.Milestone)
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return Result<bool>.Failure("Task not found");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.WorkspaceId != task.Project?.WorkspaceId)
                return Result<bool>.Failure("Unauthorized: You do not belong to the project's workspace.");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == userId);

            // ✅ Safe null checks
            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == userId;
            bool isProjectManager = task.Project?.CreatedByUserId == userId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<bool>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can restore tasks.");
            }

            if (task.Milestone != null && task.Milestone.IsDeleted)
            {
                return Result<bool>.Failure("Cannot restore task. The parent Milestone is deleted. Restore the milestone first.");
            }

            if (task.Project != null && task.Project.IsDeleted)
            {
                return Result<bool>.Failure("Cannot restore task. The parent Project is deleted. Restore the project first.");
            }

            if (!string.IsNullOrEmpty(task.CategoryId))
            {
                var category = await _unitOfWork.Repository<TaskCategory>().Query()
                    .IgnoreQueryFilters() 
                    .FirstOrDefaultAsync(c => c.Id == task.CategoryId);

                if (category != null && category.IsDeleted)
                {
                    task.CategoryId = null;
                }
            }

            task.IsDeleted = false;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task restored successfully");
        }

        // ADD DEPENDENCY
        public async Task<Result<bool>> AddDependencyAsync(AddTaskDependencyDto dto, string currentUserId)
        {
            if (dto.TaskId == dto.DependsOnTaskId)
                return Result<bool>.Failure("Task cannot depend on itself");

            var task = await _unitOfWork.Repository<TaskItem>().Query()
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == dto.TaskId);

            var dependsOnTask = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(dto.DependsOnTaskId);

            if (task == null || dependsOnTask == null)
                return Result<bool>.Failure("One or both tasks not found");

            if (task.ProjectId != dependsOnTask.ProjectId)
                return Result<bool>.Failure("Tasks must belong to the same project to create a dependency.");

            if (task.IsDeleted || dependsOnTask.IsDeleted)
                return Result<bool>.Failure("Cannot create dependency for deleted tasks.");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == currentUserId);

            // ✅ Safe null checks
            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectManager = task.Project?.CreatedByUserId == currentUserId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<bool>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can manage task dependencies.");
            }

            var exists = await _unitOfWork.Repository<TaskDependency>().Query()
                .AnyAsync(d => d.TaskId == dto.TaskId && d.DependsOnTaskId == dto.DependsOnTaskId);

            if (exists)
                return Result<bool>.Failure("Dependency already exists.");

            var hasCircular = await HasCircularDependencyAsync(dto.TaskId, dto.DependsOnTaskId);
            if (hasCircular)
                return Result<bool>.Failure("Circular dependency detected! This would create a dependency loop.");

            var dependency = new TaskDependency
            {
                TaskId = dto.TaskId,
                DependsOnTaskId = dto.DependsOnTaskId
            };

            await _unitOfWork.Repository<TaskDependency>().AddAsync(dependency);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Dependency added successfully");
        }

        //CONFIRMATION
        public async Task<Result<bool>> ConfirmTaskAsync(string taskId, string currentUserId)
        {
            var task = await _unitOfWork.Repository<TaskItem>().Query()
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return Result<bool>.Failure("Task not found");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == currentUserId);

            // ✅ Safe null checks
            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectManager = task.Project?.CreatedByUserId == currentUserId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<bool>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can confirm tasks.");
            }

            if (task.Status != TaskStatus.Submitted)
            {
                return Result<bool>.Failure("Task must be submitted by the employee before confirmation.");
            }

            task.Status = TaskStatus.Completed;
            task.ReviewedAt = DateTime.UtcNow;
            task.ReviewedByUserId = currentUserId;

            _unitOfWork.Repository<TaskItem>().Update(task);

            var dependentTasks = await _unitOfWork.Repository<TaskDependency>().Query()
                .Include(d => d.Task)
                .Where(d => d.DependsOnTaskId == taskId)
                .ToListAsync();

            await _unitOfWork.SaveChangesAsync();

            foreach (var dep in dependentTasks)
            {
                if (dep.Task != null && !string.IsNullOrEmpty(dep.Task.AssignedToUserId))
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = dep.Task.AssignedToUserId,
                        TriggeredByUserId = currentUserId,
                        Title = "Dependency Resolved",
                        Message = $"The prerequisite task '{task.Title}' is completed. You can now start '{dep.Task.Title}'.",
                        Type = NotificationType.System
                    });
                }
            }

            if (!string.IsNullOrEmpty(task.AssignedToUserId))
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = task.AssignedToUserId,
                    TriggeredByUserId = currentUserId,
                    Title = "Task Accepted",
                    Message = $"Your work on '{task.Title}' has been reviewed and accepted.",
                    Type = NotificationType.System
                });
            }

            return Result<bool>.Success(true, "Task confirmed and notifications sent.");
        }

        //Rejection
        public async Task<Result<bool>> RejectTaskAsync(string taskId, string currentUserId, string comment)
        {
            var task = await _unitOfWork.Repository<TaskItem>().Query()
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Workspace)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return Result<bool>.Failure("Task not found");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == task.ProjectId && m.UserId == currentUserId);

            // ✅ Safe null checks
            bool isWorkspaceOwner = task.Project?.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectManager = task.Project?.CreatedByUserId == currentUserId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<bool>.Failure(
                    "Unauthorized: Only Workspace Owner, Project Manager, or Team Leader can reject tasks.");
            }

            if (task.Status != TaskStatus.Submitted)
            {
                return Result<bool>.Failure("Only submitted tasks can be rejected.");
            }

            task.Status = TaskStatus.Rejected;
            task.ReviewComment = comment;
            task.ReviewedAt = DateTime.UtcNow;
            task.ReviewedByUserId = currentUserId;

            _unitOfWork.Repository<TaskItem>().Update(task);
            
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrEmpty(task.AssignedToUserId))
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = task.AssignedToUserId,
                    TriggeredByUserId = currentUserId,
                    Title = "Task Rejected",
                    Message = $"Your work on '{task.Title}' was rejected. Reason: {comment}. Please revise and start again.",
                    Type = NotificationType.System,
                    RelatedEntityId = task.Id
                });
            }

            return Result<bool>.Success(true, "Task status set to Rejected.");
        }

        //FilterTasks
        public async Task<Result<List<TaskResponseDto>>> FilterTasksAsync(TaskFilterDto filter, string currentUserId)
        {
            var authorizedProjectIds = await _unitOfWork.Repository<SyncVerse.Domain.Entities.Project>()
                .Query()
                .Include(p => p.Workspace)
                .Include(p => p.TeamMembers)
                .Where(p => p.CreatedByUserId == currentUserId ||
                            (p.Workspace != null && p.Workspace.CreatedByUserId == currentUserId) ||
                            p.TeamMembers.Any(m => m.UserId == currentUserId &&
                                             (m.Role == ProjectRole.ProjectManager || m.Role == ProjectRole.TeamLeader)))
                .Select(p => p.Id)
                .ToListAsync();

            var query = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Project)
                .Include(t => t.Milestone)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Category)
                .Where(t => t.ProjectId != null && authorizedProjectIds.Contains(t.ProjectId!))
                .AsQueryable();


            if (!string.IsNullOrEmpty(filter.ProjectId))
                query = query.Where(t => t.ProjectId == filter.ProjectId);

            if (!string.IsNullOrEmpty(filter.MilestoneId))
                query = query.Where(t => t.MilestoneId == filter.MilestoneId);

            if (!string.IsNullOrEmpty(filter.CategoryId))
                query = query.Where(t => t.CategoryId == filter.CategoryId);

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.AssignedUserId))
                query = query.Where(t => t.AssignedToUserId == filter.AssignedUserId);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.DueDate != null && t.DueDate <= filter.ToDate.Value);

            var tasks = await query.ToListAsync();

            return Result<List<TaskResponseDto>>.Success(_mapper.Map<List<TaskResponseDto>>(tasks));
        }

        // STATUS TRANSITION VALIDATION
        private bool IsValidStatusTransition(TaskStatus current, TaskStatus next)
        {
            if (current == next)
                return true;

            return current switch
            {
                TaskStatus.Pending => next == TaskStatus.InProgress,
                TaskStatus.InProgress => next == TaskStatus.Submitted || next == TaskStatus.Pending,
                TaskStatus.Submitted => next == TaskStatus.Completed || next == TaskStatus.Rejected,
                TaskStatus.Rejected => next == TaskStatus.InProgress,
                TaskStatus.Completed => false,
                _ => false
            };
        }

        // CIRCULAR DEPENDENCY DETECTION (DFS-based)
        private async Task<bool> HasCircularDependencyAsync(string taskId, string dependsOnTaskId)
        {
            var visited = new HashSet<string>();
            return await DfsCheckCircularAsync(dependsOnTaskId, taskId, visited);
        }

        private async Task<bool> DfsCheckCircularAsync(string currentTaskId, string targetTaskId, HashSet<string> visited)
        {
            if (currentTaskId == targetTaskId)
                return true;

            if (visited.Contains(currentTaskId))
                return false;

            visited.Add(currentTaskId);

            var dependencies = await _unitOfWork.Repository<TaskDependency>().Query()
                .Where(d => d.TaskId == currentTaskId)
                .Select(d => d.DependsOnTaskId)
                .ToListAsync();

            foreach (var depTaskId in dependencies)
            {
                if (await DfsCheckCircularAsync(depTaskId, targetTaskId, visited))
                    return true;
            }

            return false;
        }


        public async Task<List<UnityTaskResponseDto>> GetUnityTasksAsync(string orgId, string teamId)
        {
            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Project)
                .Where(t => t.Project != null && t.Project.WorkspaceId == orgId && t.Project.TeamId == teamId && !t.IsDeleted);

            var tasks = await tasksQuery.ToListAsync();

            var result = tasks.Select(t => new UnityTaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = MapStatusToUnity(t.Status),
                AssigneeId = t.AssignedToUserId
            }).ToList();

            return result;
        }


        public async Task<bool> UpdateTaskStatusAsync(string taskId, string status)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);
            if (task == null || task.IsDeleted)
                return false;

            var mappedStatus = MapUnityStatusToTaskStatus(status);
            if (mappedStatus == null)
                return false;

            task.Status = mappedStatus.Value;
            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string MapStatusToUnity(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.Pending => "Todo",
                TaskStatus.InProgress => "Doing",
                TaskStatus.Completed => "Done",
                _ => "Todo"
            };
        }

        private TaskStatus? MapUnityStatusToTaskStatus(string status)
        {
            return status switch
            {
                "Todo" => TaskStatus.Pending,
                "Doing" => TaskStatus.InProgress,
                "Done" => TaskStatus.Completed,
                _ => null
            };
        }

        public async Task<Result<List<TaskResponseDto>>> SaveExtractedTasksAsync(
    List<SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction.AiExtractedTaskDto> extractedTasks,
    string projectId,
    string milestoneId,
    string currentUserId)
        {

            var project = await _unitOfWork.Repository<ProjectEntity>().Query()
                .Include(p => p.Workspace)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return Result<List<TaskResponseDto>>.Failure("Project not found");

            var milestone = await _unitOfWork.Repository<Milestone>().GetByIdAsync(milestoneId);
            if (milestone == null) return Result<List<TaskResponseDto>>.Failure("Milestone not found");

            var projectMember = await _unitOfWork.Repository<ProjectMember>().Query()
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == currentUserId);

            bool isWorkspaceOwner = project.Workspace?.CreatedByUserId == currentUserId;
            bool isProjectManager = project.CreatedByUserId == currentUserId;
            bool isTeamLeader = projectMember?.Role == ProjectRole.TeamLeader;

            if (!isWorkspaceOwner && !isProjectManager && !isTeamLeader)
            {
                return Result<List<TaskResponseDto>>.Failure("Unauthorized: You don't have permission to create tasks in this project.");
            }

            var members = await _unitOfWork.Repository<ProjectMember>().Query()
                .Include(m => m.User)
                .Where(m => m.ProjectId == projectId && m.User != null)
                .Select(m => m.User!)
                .ToListAsync();

            var savedTasks = new List<TaskItem>();

            foreach (var aiTask in extractedTasks)
            {
                var matchedUser = members.FirstOrDefault(u =>
                    (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.Contains(aiTask.Assignee, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.LastName) && u.LastName.Contains(aiTask.Assignee, StringComparison.OrdinalIgnoreCase))
                );

                var priority = TaskPriority.Medium; 
                if (Enum.TryParse<TaskPriority>(aiTask.Priority, true, out var parsedPriority))
                {
                    priority = parsedPriority;
                }

                DateTime? finalDueDate = null;
                if (!string.IsNullOrEmpty(aiTask.Deadline) && DateTime.TryParse(aiTask.Deadline, out var parsedDate))
                {
                    if (parsedDate >= milestone.StartDate && parsedDate <= milestone.EndDate)
                    {
                        finalDueDate = parsedDate;
                    }
                }

                if (finalDueDate == null)
                {
                    finalDueDate = milestone.EndDate.AddDays(-1);
                }

                var taskItem = new TaskItem
                {
                    Title = string.IsNullOrWhiteSpace(aiTask.Title) ? "AI Extracted Task" : aiTask.Title,
                    Description = $"{aiTask.Description}\n\n[AI Source Quote]: \"{aiTask.SourceQuote}\"",
                    Status = TaskStatus.Pending,
                    CreatedByUserId = currentUserId,
                    AssignedToUserId = matchedUser != null ? matchedUser.Id : currentUserId, 
                    ProjectId = projectId,
                    MilestoneId = milestoneId,
                    DueDate = finalDueDate,
                    Priority = priority,
                    WorkspaceId = project.WorkspaceId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<TaskItem>().AddAsync(taskItem);
                savedTasks.Add(taskItem);
            }

            await _unitOfWork.SaveChangesAsync();

            foreach (var savedTask in savedTasks)
            {
                if (savedTask.AssignedToUserId != currentUserId)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = savedTask.AssignedToUserId,
                        TriggeredByUserId = currentUserId,
                        Title = "New Task Assigned via AI",
                        Message = $"You have been assigned a new task extracted from the meeting: {savedTask.Title}",
                        Type = NotificationType.TaskAssigned,
                        RelatedEntityId = savedTask.Id
                    });
                }
            }

            var responseDtos = _mapper.Map<List<TaskResponseDto>>(savedTasks);
            return Result<List<TaskResponseDto>>.Success(responseDtos, $"{savedTasks.Count} tasks saved successfully from AI extraction.");
        }

    }
}