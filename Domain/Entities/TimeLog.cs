using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Models;

namespace Synverse.Domain.Entities
{
    public class TimeLog : BaseEntity
    {
        public string? TaskId { get; set; }
        public string? UserId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } 

        public int DurationInMinutes { get; set; }
        public bool IsManual { get; set; }

        // Navigation
        public TaskEmployee Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
