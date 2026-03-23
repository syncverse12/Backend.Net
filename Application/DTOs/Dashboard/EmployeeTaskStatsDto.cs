namespace SyncVerse.Application.DTOs.Dashboard
{
    public class EmployeeTaskStatsDto
    {
        public string EmployeeId { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public int TasksCount { get; set; }
    }
}
