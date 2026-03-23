namespace SyncVerse.Application.DTOs.Dashboard
{
    public class ManagerTaskDashboardDto
    {
        public TaskStatusStatsDto StatusStats { get; set; } = null!;
        public List<EmployeeTaskStatsDto> TasksPerEmployee { get; set; } = null!;
        public List<CategoryTaskStatsDto> TasksPerCategory { get; set; } = new();
    }
}

