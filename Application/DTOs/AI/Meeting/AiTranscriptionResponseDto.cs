namespace SyncVerse.Application.DTOs.AI.Meeting
{
    public class AiTranscriptionResponseDto
    {
        public string Text { get; set; } = null!;
        public string? Language { get; set; }
        public double? Confidence { get; set; }
        public double? DurationSeconds { get; set; }
    }
}