using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction
{
    public class AiTaskExtractionResponseDto
    {
        [JsonPropertyName("meeting_id")]
        public int MeetingId { get; set; }

        [JsonPropertyName("tasks")]
        public List<AiExtractedTaskDto> Tasks { get; set; } = new();

        [JsonPropertyName("tasks_count")]
        public int TasksCount { get; set; }

        [JsonPropertyName("processing_notes")]
        public List<string> ProcessingNotes { get; set; } = new();
    }
}
