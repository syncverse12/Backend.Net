using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskMetadataDto
    {
        public DateTime GeneratedAt { get; set; }
        public string ProjectId { get; set; } = string.Empty;
        public string AnalysisVersion { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double ExecutionTimeMs { get; set; }
        public double DataCompleteness { get; set; }
        public string CollectionMode { get; set; } = string.Empty;
        public List<string> MissingSources { get; set; } = new();
    }
}
