namespace SyncVerse.Application.DTOs.Dashboard
{
    public class TeamLeaderDashboardDto
    {
        public List<TeamWorkloadDto> TeamWorkload { get; set; } = new List<TeamWorkloadDto>();
        public List<PendingReviewDto> PendingReviews { get; set; } = new List<PendingReviewDto>();
        public List<BlockerDto> BlockersRadar { get; set; } = new List<BlockerDto>();
        public TeamVelocityDto TeamVelocity { get; set; } = new TeamVelocityDto();
    }

    public class TeamWorkloadDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int TotalActiveTasks => PendingTasks + InProgressTasks;
    }

    public class PendingReviewDto
    {
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string SubmittedByUserId { get; set; } = string.Empty;
        public string SubmittedByUserName { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }

    public class BlockerDto
    {
        public string TaskId { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty; // e.g. "Overdue", "Rejected", "Blocked"
        public string AssignedToUserId { get; set; } = string.Empty;
        public string AssignedToUserName { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
    }

    public class TeamVelocityDto
    {
        public int CompletedThisWeek { get; set; }
        public int CompletedLastWeek { get; set; }
        public double GrowthPercentage { get; set; }
    }
}