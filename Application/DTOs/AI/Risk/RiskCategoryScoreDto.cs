namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskCategoryScoreDto
    {
        public string Category { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<string> ContributingFactors { get; set; } = new();
        public double Weight { get; set; }
    }
}
