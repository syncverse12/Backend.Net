using System.Collections.Generic;

namespace SyncVerse.Application.Interfaces.Notifications
{
    public interface IRealtimeNotificationService
    {
        System.Threading.Tasks.Task SendNotificationToUserAsync(string userId, object notification);
        System.Threading.Tasks.Task SendNotificationMarkedAsReadAsync(string userId, string notificationId);
        System.Threading.Tasks.Task SendBulkNotificationAsync(List<string> userIds, object notification);
    }
}