using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class OverallRiskDto
    {
        public int Score { get; set; }
        public string Level { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}
