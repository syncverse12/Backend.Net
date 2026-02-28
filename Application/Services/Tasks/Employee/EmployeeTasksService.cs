using AutoMapper;
using Graduation_Project.Application.Common.Models;
using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Notifications;
using Graduation_Project.Application.DTOs.Tasks.Employee;
using Graduation_Project.Application.DTOs.Tasks.Manager;
using Graduation_Project.Application.Interfaces.Identity;
using Graduation_Project.Application.Interfaces.Notifications;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Tasks.Employee;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Graduation_Project.Application.Services.Task.Employee
{
    public class EmployeeTaskService : IEmployeeTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public EmployeeTaskService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<Result<PagedResult<TaskResponseDto>>> GetMyTasksAsync(string userId, TaskQuery query)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var tasksQuery = _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Category)
                .Where(t =>
                    t.AssignedToUserId == employeeId &&
                    !t.IsDeleted);

            if (query.Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);

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
                    PageNumber = query.Page,
                    PageSize = query.PageSize
                });
        }

        public async Task<Result<EmployeeTaskDetailsDto>> GetTaskDetailsAsync(string taskId, string userId)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Project)
                .Include(t => t.Milestone)
                .Include(t => t.Category)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.ReviewedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<EmployeeTaskDetailsDto>.Failure("Task not found");

            if (task.AssignedToUserId != employeeId)
                return Result<EmployeeTaskDetailsDto>.Failure("Unauthorized: You don't have access to this task");

            var details = new EmployeeTaskDetailsDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                ProjectName = task.Project?.Name,
                ProjectId = task.ProjectId,
                MilestoneName = task.Milestone?.Name,
                MilestoneId = task.MilestoneId,
                CategoryName = task.Category?.Name,
                CategoryId = task.CategoryId,
                ReviewComment = task.ReviewComment,
                ReviewedAt = task.ReviewedAt,
                SubmissionLink = task.SubmissionLink,
                SubmissionNotes = task.SubmissionNotes,
                SubmittedAt = task.SubmittedAt,
                CreatedByName = task.CreatedByUser?.UserName ?? "Unknown",
                AssignedToName = task.AssignedToUser?.UserName ?? "Unknown"
            };

            return Result<EmployeeTaskDetailsDto>.Success(details);
        }

        public async Task<Result<bool>> StartTaskAsync(string taskId, string userId)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .GetByIdAsync(taskId);

            if (task == null)
                return Result<bool>.Failure("Task not found");

            if (task.AssignedToUserId != employeeId)
                return Result<bool>.Failure("Unauthorized");

            if (task.Status != TaskStatus.Pending && task.Status != TaskStatus.Rejected)
                return Result<bool>.Failure("Task cannot be started from its current status.");

            task.Status = TaskStatus.InProgress;
            task.TaskStartedAt = DateTime.UtcNow;

            var activeTimer = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .FirstOrDefaultAsync(t => t.TaskId == taskId && 
                                         t.UserId == employeeId && 
                                         t.EndTime == null && 
                                         !t.IsDeleted);

            if (activeTimer == null)
            {
                var timeLog = new TimeLog
                {
                    TaskId = taskId,
                    UserId = employeeId,
                    StartTime = DateTime.UtcNow,
                    IsManual = false,
                    Notes = "Auto-started with task"
                };

                await _unitOfWork.Repository<TimeLog>().AddAsync(timeLog);
            }

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Task started and timer activated");
        }

        public async Task<Result<bool>> SubmitTaskAsync(string taskId, string userId, SubmitTaskDto submitDto)
        {
            var employeeId = string.IsNullOrEmpty(userId) ? _currentUser.UserId : userId;

            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return Result<bool>.Failure("Task not found");
            if (task.AssignedToUserId != employeeId) return Result<bool>.Failure("Unauthorized");

            if (task.Status == TaskStatus.Submitted)
                return Result<bool>.Failure("Task is already submitted.");

            task.Status = TaskStatus.Submitted;
            task.SubmissionLink = submitDto.SubmissionLink;
            task.SubmissionNotes = submitDto.SubmissionNotes;
            task.SubmittedAt = DateTime.UtcNow;
            task.TaskCompletedAt = DateTime.UtcNow;

            var activeTimer = await _unitOfWork.Repository<TimeLog>()
                .Query()
                .FirstOrDefaultAsync(t => t.TaskId == taskId && 
                                         t.UserId == employeeId && 
                                         t.EndTime == null && 
                                         !t.IsDeleted);

            if (activeTimer != null)
            {
                activeTimer.EndTime = DateTime.UtcNow;
                activeTimer.DurationInMinutes = (int)(activeTimer.EndTime.Value - activeTimer.StartTime).TotalMinutes;
                activeTimer.Notes = string.IsNullOrEmpty(activeTimer.Notes) 
                    ? "Auto-stopped with task submission" 
                    : activeTimer.Notes + " (Auto-stopped)";

                _unitOfWork.Repository<TimeLog>().Update(activeTimer);
            }

            _unitOfWork.Repository<TaskItem>().Update(task);
            await _unitOfWork.SaveChangesAsync();

            if (task.Project != null)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = task.Project.CreatedByUserId,
                    TriggeredByUserId = employeeId,
                    Title = "Task Submitted",
                    Message = $"The task '{task.Title}' has been submitted for review.",
                    Type = NotificationType.System,
                    RelatedEntityId = task.Id
                });
            }

            return Result<bool>.Success(true, "Task submitted for review");
        }

    }
}
