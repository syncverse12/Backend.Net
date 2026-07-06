namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class HistoricalCaseDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string KeyLesson { get; set; } = string.Empty;
    }
}
