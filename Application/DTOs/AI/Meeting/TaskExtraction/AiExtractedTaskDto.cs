using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction
{
    public class AiExtractedTaskDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("assignee")]
        public string Assignee { get; set; } = string.Empty;

        [JsonPropertyName("deadline")]
        public string? Deadline { get; set; } 

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "MEDIUM";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "general";

        [JsonPropertyName("estimated_hours")]
        public object? EstimatedHours { get; set; }

        [JsonPropertyName("source_quote")]
        public string SourceQuote { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}
