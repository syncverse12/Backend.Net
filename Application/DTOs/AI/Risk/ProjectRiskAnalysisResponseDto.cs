using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskAnalysisResponseDto
    {
        public OverallRiskDto OverallRisk { get; set; } = new();
        public List<RiskCategoryDto> RiskCategories { get; set; } = new();
        public List<MetricDto> CalculatedMetrics { get; set; } = new();
        public List<AiEstimatedMetricDto> AiEstimatedMetrics { get; set; } = new();
        public List<RiskRecommendationDto> Recommendations { get; set; } = new();
        public RiskMetadataDto Metadata { get; set; } = new();
    }
}
