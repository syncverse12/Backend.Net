using SyncVerse.API.Hubs;
using SyncVerse.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.SignalR;


namespace SyncVerse.Infrastructure.Realtime
{
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(string userId, object notification)
        {
            try
            {
                await _hubContext.Clients.Group(userId)
                    .SendAsync("ReceiveNotification", notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR failed: {ex.Message}");
            }
        }

        public async Task SendNotificationMarkedAsReadAsync(string userId, string notificationId)
        {
            try
            {
                await _hubContext.Clients.Group(userId)
                    .SendAsync("NotificationMarkedAsRead", notificationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR failed: {ex.Message}");
            }
        }

        public async Task SendBulkNotificationAsync(List<string> userIds, object notification)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await _hubContext.Clients.Group(userId)
                        .SendAsync("ReceiveNotification", notification);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR bulk failed: {ex.Message}");
            }
        }
    }
}
