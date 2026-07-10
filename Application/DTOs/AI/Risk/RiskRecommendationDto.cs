using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskRecommendationDto
    {
        public string Priority { get; set; } = string.Empty;
        public string RelatedRisk { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}
