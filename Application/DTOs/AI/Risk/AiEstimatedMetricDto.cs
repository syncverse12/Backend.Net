using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class AiEstimatedMetricDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
