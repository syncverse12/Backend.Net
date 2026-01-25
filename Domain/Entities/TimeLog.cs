using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Models;

namespace Synverse.Domain.Entities
{
    public class TimeLog : BaseEntity
    {
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; } 

        public int DurationInMinutes { get; set; }
        public bool IsManual { get; set; }

        // Navigation
        public Task Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
