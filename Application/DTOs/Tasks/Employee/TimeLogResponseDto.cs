namespace Graduation_Project.Application.DTOs.Tasks.Employee
{
    public class TimeLogResponseDto
    {
        public string TimeLogId { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public string TaskTitle { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationInMinutes { get; set; }
        public bool IsManual { get; set; }
        public string? Notes { get; set; }
    }
}
