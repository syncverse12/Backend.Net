using System;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class LiveRiskUpdateRequestDto
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("snapshot_at")]
        public string SnapshotAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        [JsonPropertyName("sprint_velocity")]
        public int SprintVelocity { get; set; }

        [JsonPropertyName("planned_velocity")]
        public int PlannedVelocity { get; set; }

        [JsonPropertyName("sprint_completion_rate")]
        public double SprintCompletionRate { get; set; }

        [JsonPropertyName("overdue_tasks")]
        public int OverdueTasks { get; set; }

        [JsonPropertyName("total_tasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("blocked_tasks")]
        public int BlockedTasks { get; set; }

        [JsonPropertyName("task_reassignment_count")]
        public int TaskReassignmentCount { get; set; }

        [JsonPropertyName("github_commits_last_7d")]
        public int GithubCommitsLast7d { get; set; }

        [JsonPropertyName("pr_open_count")]
        public int PrOpenCount { get; set; }

        [JsonPropertyName("pr_avg_review_hours")]
        public double PrAvgReviewHours { get; set; }

        [JsonPropertyName("deployment_failures_last_30d")]
        public int DeploymentFailuresLast30d { get; set; }

        [JsonPropertyName("qa_failure_rate")]
        public double QaFailureRate { get; set; }

        [JsonPropertyName("team_overtime_hours_avg")]
        public double TeamOvertimeHoursAvg { get; set; }

        [JsonPropertyName("team_absences_count")]
        public int TeamAbsencesCount { get; set; }

        [JsonPropertyName("negative_sentiment_score")]
        public double NegativeSentimentScore { get; set; }

        [JsonPropertyName("client_alignment_score")]
        public double ClientAlignmentScore { get; set; }

        [JsonPropertyName("client_response_delay_hours")]
        public double ClientResponseDelayHours { get; set; }

        [JsonPropertyName("unresolved_client_feedback")]
        public int UnresolvedClientFeedback { get; set; }
    }
}