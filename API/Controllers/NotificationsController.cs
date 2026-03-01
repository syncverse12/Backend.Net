using SyncVerse.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SyncVerse.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(string notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
