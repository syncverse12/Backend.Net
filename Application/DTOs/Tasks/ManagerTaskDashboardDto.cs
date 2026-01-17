namespace Graduation_Project.Application.DTOs.Tasks
{
    public class ManagerTaskDashboardDto
    {
        public TaskStatusStatsDto StatusStats { get; set; } = null!;
        public List<EmployeeTaskStatsDto> TasksPerEmployee { get; set; } = null!;
    }

}
