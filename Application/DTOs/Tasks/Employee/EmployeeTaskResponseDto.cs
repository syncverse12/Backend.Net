using SyncVerse.Domain.Enums;

namespace SyncVerse.Application.DTOs.Tasks.Employee
{
    public class EmployeeTaskResponseDto
    {
        public string TaskId { get; set; } = null!;
        public string TaskTitle { get; set; } = null!;
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public int ProgressPercentage { get; set; }
    }

}
