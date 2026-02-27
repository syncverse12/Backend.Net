namespace Graduation_Project.Application.DTOs.Tasks.Employee
{
    public class TaskTimeStatsDto
    {
        public string TaskId { get; set; } = null!;
        public string TaskTitle { get; set; } = null!;
        
        public int ActiveWorkingTimeMinutes { get; set; }
        public string ActiveWorkingTimeFormatted { get; set; } = null!;
        
        public int? TotalDurationMinutes { get; set; }
        public string? TotalDurationFormatted { get; set; }
        
        public DateTime? TaskStartedAt { get; set; }
        public DateTime? TaskCompletedAt { get; set; }
        
        public int SessionsCount { get; set; }
    }
}
