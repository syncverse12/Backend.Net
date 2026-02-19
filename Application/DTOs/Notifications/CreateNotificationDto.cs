using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Application.DTOs.Notifications
{
    public class CreateNotificationDto
    {
        public string UserId { get; set; } = null!;
        public string TriggeredByUserId { get; set; } = null!; 
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? TaskId { get; set; }
        public string? RelatedEntityId { get; set; }
        public string? ActionUrl { get; set; }
    }
}
