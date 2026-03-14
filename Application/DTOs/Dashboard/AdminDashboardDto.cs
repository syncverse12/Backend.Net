namespace SyncVerse.Application.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalManagers { get; set; }
        public int TotalEmployees { get; set; }

        public int TotalWorkspaces { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }

        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public int OverdueTasks { get; set; }
    }
}
