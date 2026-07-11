using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Source { get; set; } = string.Empty;
        public List<string> UsedMetrics { get; set; } = new();
    }
}
