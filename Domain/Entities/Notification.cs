using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Enums;

namespace Graduation_Project.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public string? TriggeredByUserId { get; set; }
        public User? TriggeredByUser { get; set; }

        public NotificationType Type { get; set; }
        
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public string? TaskId { get; set; }
        public TaskItem? Task { get; set; }

        public string? RelatedEntityId { get; set; }

        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
    }
}
