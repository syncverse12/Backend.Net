using System;
using System.Collections.Generic;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskResponseDto
    {
        public string ReportId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string GeneratedAt { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public RiskScoresDto Scores { get; set; } = new();

        public double DelayProbability { get; set; }
        public double BudgetOverrunProbability { get; set; }
        public double DeliveryConfidence { get; set; }
        public double BurnoutProbability { get; set; }
        public string ExecutiveSummary { get; set; } = string.Empty;

        public List<string> RootCauses { get; set; } = new();
        public List<string> PredictedConsequences { get; set; } = new();
        public List<MitigationPlanDto> MitigationPlan { get; set; } = new();
        public List<HistoricalCaseDto> SimilarHistoricalCases { get; set; } = new();
        public string MlModelVersion { get; set; } = string.Empty;
    }
}