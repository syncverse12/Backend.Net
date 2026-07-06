namespace SyncVerse.Application.DTOs.AI
{
    public class AttritionPredictionResponseDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public double AttritionProbability { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<RiskFactorDto> TopRiskFactors { get; set; } = new();
        public List<RecommendationDto> Recommendations { get; set; } = new();
        public string ExplanationSummary { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public DateTime PredictedAt { get; set; }
    }

    public class RiskFactorDto
    {
        public string Feature { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public double Impact { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class RecommendationDto
    {
        public string Priority { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ExpectedImpact { get; set; } = string.Empty;
    }
}