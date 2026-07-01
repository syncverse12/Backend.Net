namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskResponseDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public RiskScoresDto Scores { get; set; } = new();
        public int DelayProbability { get; set; }
        public int BudgetOverrunProbability { get; set; }
        public int DeliveryConfidence { get; set; }
        public int BurnoutProbability { get; set; }
        public string ExecutiveSummary { get; set; } = string.Empty;
        public List<string> RootCauses { get; set; } = new();
        public List<string> PredictedConsequences { get; set; } = new();
        public List<MitigationPlanDto> MitigationPlan { get; set; } = new();
        public List<SimilarCaseDto> SimilarHistoricalCases { get; set; } = new();
        public string MlModelVersion { get; set; } = string.Empty;
    }
}
