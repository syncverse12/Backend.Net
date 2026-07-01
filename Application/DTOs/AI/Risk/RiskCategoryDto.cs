namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class RiskCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Severity { get; set; } = string.Empty;
        public List<string> ContributingFactors { get; set; } = new();
        public int Weight { get; set; }
    }
}
