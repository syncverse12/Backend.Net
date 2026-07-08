using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskAssessmentResponseDto
    {
        [JsonPropertyName("overall_risk")]
        public OverallRiskDto OverallRisk { get; set; } = new();

        [JsonPropertyName("risk_categories")]
        public List<RiskCategoryDto> RiskCategories { get; set; } = new();

        [JsonPropertyName("calculated_metrics")]
        public List<MetricDto> CalculatedMetrics { get; set; } = new();

        [JsonPropertyName("ai_estimated_metrics")]
        public List<AiMetricDto> AiEstimatedMetrics { get; set; } = new();

        [JsonPropertyName("recommendations")]
        public List<RecommendationDto> Recommendations { get; set; } = new();
    }
}