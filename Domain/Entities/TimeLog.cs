using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Domain.Entities
{
    public class TimeLog : BaseEntity
    {
        public string TaskId { get; set; } = null!;
        public string UserId { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } 

        public int DurationInMinutes { get; set; }
        public bool IsManual { get; set; }

        public string? Notes { get; set; }

        // Navigation
        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
