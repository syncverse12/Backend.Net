using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Notifications
{
    public class NotificationResponseDto
    {
        public string NotificationId { get; set; } = null!;
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? TaskId { get; set; }
        public string? TaskTitle { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
