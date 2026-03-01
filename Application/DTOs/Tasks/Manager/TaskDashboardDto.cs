using SyncVerse.Application.DTOs.Tasks.Manager;

namespace SyncVerse.Application.DTOs.Tasks
{
    public class TaskDashboardDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public List<CategoryTaskStatsDto> CategoryStats { get; set; } = new();
    }
}
