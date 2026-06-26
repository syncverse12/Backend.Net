using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Meeting
{
    public class AiMeetingSummaryRequestDto
    {
        [JsonPropertyName("meeting_id")]
        public int MeetingId { get; set; }

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;

        [JsonPropertyName("meeting_title")]
        public string MeetingTitle { get; set; } = string.Empty;

        [JsonPropertyName("attendees")]
        public List<string> Attendees { get; set; } = new();

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en";
    }
}