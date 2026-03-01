using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Notifications;
using SyncVerse.Domain.Enums;
using System.Threading.Tasks;

namespace SyncVerse.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task<Result<NotificationResponseDto>> CreateNotificationAsync(CreateNotificationDto dto);
        Task<Result<List<NotificationResponseDto>>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task<Result<int>> GetUnreadCountAsync(string userId);
        Task<Result<bool>> MarkAsReadAsync(string notificationId, string userId);
        Task<Result<bool>> MarkAllAsReadAsync(string userId);
        Task<Result<bool>> DeleteNotificationAsync(string notificationId, string userId);

        // Helper methods for specific notification types
        System.Threading.Tasks.Task NotifyTaskAssignedAsync(string taskId, string assignedUserId, string assignedByUserName);
        System.Threading.Tasks.Task NotifyTaskCommentedAsync(string taskId, string commentUserId, string commentUserName);
        System.Threading.Tasks.Task NotifyTaskSubmittedAsync(string taskId, string submittedByUserId);
        System.Threading.Tasks.Task NotifyTaskReviewedAsync(string taskId, string reviewedByUserName, bool isApproved, string? reviewComment);
    }
}
