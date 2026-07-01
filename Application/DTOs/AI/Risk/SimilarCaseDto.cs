namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class SimilarCaseDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public int SimilarityScore { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string KeyLesson { get; set; } = string.Empty;
    }
}
