namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class LiveRiskUpdateRequestDto
    {
        public string ProjectId { get; set; } = string.Empty;
        public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;
        public int SprintVelocity { get; set; } = 0;
        public int PlannedVelocity { get; set; } = 0;
        public int SprintCompletionRate { get; set; } = 1;
        public int OverdueTasks { get; set; } = 0;
        public int TotalTasks { get; set; } = 0;
        public int BlockedTasks { get; set; } = 0;
        public int TaskReassignmentCount { get; set; } = 0;
        public int GithubCommitsLast7d { get; set; } = 0;
        public int PrOpenCount { get; set; } = 0;
        public int PrAvgReviewHours { get; set; } = 0;
        public int DeploymentFailuresLast30d { get; set; } = 0;
        public int QaFailureRate { get; set; } = 0;
        public int TeamOvertimeHoursAvg { get; set; } = 0;
        public int TeamAbsencesCount { get; set; } = 0;
        public int NegativeSentimentScore { get; set; } = 0;
        public int ClientAlignmentScore { get; set; } = 8;
        public int ClientResponseDelayHours { get; set; } = 0;
        public int UnresolvedClientFeedback { get; set; } = 0;
    }
}