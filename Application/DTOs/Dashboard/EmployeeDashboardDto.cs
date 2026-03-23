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

        public List<DailyTaskDto> TodayTasks { get; set; } = new List<DailyTaskDto>();
        public List<RecentCommentDto> RecentComments { get; set; } = new List<RecentCommentDto>();
        public List<UpcomingMilestoneDto> UpcomingMilestones { get; set; } = new List<UpcomingMilestoneDto>();

        // Role-Specific Sections
        public List<DailyTaskDto> TasksPendingMyApproval { get; set; } = new List<DailyTaskDto>(); // For Reviewers
        public List<DailyTaskDto> BugsToVerify { get; set; } = new List<DailyTaskDto>(); // For QAs
    }

    public class DailyTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }

    public class RecentCommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommentByUserId { get; set; } = string.Empty;
        public string CommentByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpcomingMilestoneDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
