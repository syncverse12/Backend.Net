using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Notifications;
using Graduation_Project.Application.Interfaces.Notifications;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Domain.Entities;
using Graduation_Project.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Application.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<NotificationResponseDto>> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                TaskId = dto.TaskId,
                RelatedEntityId = dto.RelatedEntityId,
                ActionUrl = dto.ActionUrl,
                IsRead = false
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            var task = !string.IsNullOrEmpty(dto.TaskId) 
                ? await _unitOfWork.Repository<TaskItem>().GetByIdAsync(dto.TaskId) 
                : null;

            var response = new NotificationResponseDto
            {
                NotificationId = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                TaskId = notification.TaskId,
                TaskTitle = task?.Title,
                RelatedEntityId = notification.RelatedEntityId,
                ActionUrl = notification.ActionUrl,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt
            };

            return Result<NotificationResponseDto>.Success(response);
        }

        public async Task<Result<List<NotificationResponseDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            var query = _unitOfWork.Repository<Notification>()
                .Query()
                .Include(n => n.Task)
                .Where(n => n.UserId == userId && !n.IsDeleted);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var response = notifications.Select(n => new NotificationResponseDto
            {
                NotificationId = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                TaskId = n.TaskId,
                TaskTitle = n.Task?.Title,
                RelatedEntityId = n.RelatedEntityId,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            }).ToList();

            return Result<List<NotificationResponseDto>>.Success(response);
        }

        public async Task<Result<int>> GetUnreadCountAsync(string userId)
        {
            var count = await _unitOfWork.Repository<Notification>()
                .Query()
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .CountAsync();

            return Result<int>.Success(count);
        }

        public async Task<Result<bool>> MarkAsReadAsync(string notificationId, string userId)
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .Query()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
                return Result<bool>.Failure("Notification not found");

            if (notification.IsRead)
                return Result<bool>.Success(true, "Notification already marked as read");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Notification marked as read");
        }

        public async Task<Result<bool>> MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await _unitOfWork.Repository<Notification>()
                .Query()
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            if (!unreadNotifications.Any())
                return Result<bool>.Success(true, "No unread notifications");

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, $"{unreadNotifications.Count} notifications marked as read");
        }

        public async Task<Result<bool>> DeleteNotificationAsync(string notificationId, string userId)
        {
            var notification = await _unitOfWork.Repository<Notification>()
                .Query()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification == null)
                return Result<bool>.Failure("Notification not found");

            notification.IsDeleted = true;

            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Notification deleted");
        }

        public async System.Threading.Tasks.Task NotifyTaskAssignedAsync(string taskId, string assignedUserId, string assignedByUserName)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(taskId);

            if (task == null) return;

            var notification = new CreateNotificationDto
            {
                UserId = assignedUserId,
                Type = NotificationType.TaskAssigned,
                Title = "New Task Assigned",
                Message = $"You have been assigned a new task: '{task.Title}' by {assignedByUserName}",
                TaskId = taskId,
                ActionUrl = $"/tasks/{taskId}"
            };

            await CreateNotificationAsync(notification);
        }

        public async System.Threading.Tasks.Task NotifyTaskCommentedAsync(string taskId, string commentUserId, string commentUserName)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return;

            var userIdsToNotify = new List<string>();

            if (task.AssignedToUserId != commentUserId)
                userIdsToNotify.Add(task.AssignedToUserId);

            if (task.CreatedByUserId != commentUserId && !userIdsToNotify.Contains(task.CreatedByUserId))
                userIdsToNotify.Add(task.CreatedByUserId);

            foreach (var userId in userIdsToNotify)
            {
                var notification = new CreateNotificationDto
                {
                    UserId = userId,
                    Type = NotificationType.TaskCommented,
                    Title = "New Comment on Task",
                    Message = $"{commentUserName} commented on task: '{task.Title}'",
                    TaskId = taskId,
                    ActionUrl = $"/tasks/{taskId}"
                };

                await CreateNotificationAsync(notification);
            }
        }

        public async System.Threading.Tasks.Task NotifyTaskSubmittedAsync(string taskId, string submittedByUserId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return;

            if (task.CreatedByUserId == submittedByUserId) return;

            var notification = new CreateNotificationDto
            {
                UserId = task.CreatedByUserId,
                Type = NotificationType.TaskSubmitted,
                Title = "Task Submitted for Review",
                Message = $"{task.AssignedToUser?.UserName ?? "Employee"} has submitted task: '{task.Title}' for review",
                TaskId = taskId,
                ActionUrl = $"/tasks/{taskId}"
            };

            await CreateNotificationAsync(notification);
        }

        public async System.Threading.Tasks.Task NotifyTaskReviewedAsync(string taskId, string reviewedByUserName, bool isApproved, string? reviewComment)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return;

            var notificationType = isApproved ? NotificationType.TaskApproved : NotificationType.TaskRejected;
            var title = isApproved ? "Task Approved" : "Task Rejected";
            var message = isApproved 
                ? $"Your task '{task.Title}' has been approved by {reviewedByUserName}"
                : $"Your task '{task.Title}' has been rejected by {reviewedByUserName}";

            if (!string.IsNullOrWhiteSpace(reviewComment))
                message += $". Comment: {reviewComment}";

            var notification = new CreateNotificationDto
            {
                UserId = task.AssignedToUserId,
                Type = notificationType,
                Title = title,
                Message = message,
                TaskId = taskId,
                ActionUrl = $"/tasks/{taskId}"
            };

            await CreateNotificationAsync(notification);
        }
    }
}
