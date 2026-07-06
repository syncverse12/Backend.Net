namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskScoresDto
    {
        public double Overall { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<RiskCategoryScoreDto> Categories { get; set; } = new();
        public double Confidence { get; set; }
    }
}
