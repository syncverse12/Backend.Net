using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskResponseDto
    {
        [JsonPropertyName("report_id")]
        public string ReportId { get; set; } = string.Empty;

        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("generated_at")]
        public DateTime GeneratedAt { get; set; }

        [JsonPropertyName("report_type")]
        public string ReportType { get; set; } = string.Empty;

        [JsonPropertyName("scores")]
        public RiskScoresDto Scores { get; set; } = new();

        [JsonPropertyName("delay_probability")]
        public double DelayProbability { get; set; }

        [JsonPropertyName("budget_overrun_probability")]
        public double BudgetOverrunProbability { get; set; }

        [JsonPropertyName("delivery_confidence")]
        public double DeliveryConfidence { get; set; }

        [JsonPropertyName("burnout_probability")]
        public double BurnoutProbability { get; set; }

        [JsonPropertyName("executive_summary")]
        public string ExecutiveSummary { get; set; } = string.Empty;

        [JsonPropertyName("root_causes")]
        public List<string> RootCauses { get; set; } = new();

        [JsonPropertyName("predicted_consequences")]
        public List<string> PredictedConsequences { get; set; } = new();

        [JsonPropertyName("mitigation_plan")]
        public List<MitigationPlanDto> MitigationPlan { get; set; } = new();

        [JsonPropertyName("similar_historical_cases")]
        public List<SimilarCaseDto> SimilarHistoricalCases { get; set; } = new();

        [JsonPropertyName("ml_model_version")]
        public string MlModelVersion { get; set; } = string.Empty;
    }
}