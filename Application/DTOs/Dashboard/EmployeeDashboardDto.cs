namespace SyncVerse.Application.DTOs.Dashboard
{
    public class EmployeeDashboardDto
    {
        public int MyProjectsCount { get; set; }

        public int MyTasksTotal { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public int OverdueTasks { get; set; }

        public int UnreadNotifications { get; set; }
        public int UploadedFilesCount { get; set; }
        public DateTime? NextDueDate { get; set; }
    }
}
