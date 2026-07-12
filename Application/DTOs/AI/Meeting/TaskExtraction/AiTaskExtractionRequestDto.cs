using System.Text.Json.Serialization; 

namespace SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction
{
    public class AiTaskExtractionRequestDto
    {
        [JsonPropertyName("meeting_id")] 
        public string MeetingId { get; set; } = string.Empty;

        [JsonPropertyName("transcript")] 
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("signature")] 
        public string Signature { get; set; } = string.Empty;
    }
}