using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Meeting.TaskExtraction
{
    public class AiTaskExtractionRequestDto
    {
        [JsonPropertyName("meeting_id")]
        public int MeetingId { get; set; }

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("attendees")]
        public List<string> Attendees { get; set; } = new();

        [JsonPropertyName("meeting_date")]
        public string MeetingDate { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}