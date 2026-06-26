using System.Text.Json.Serialization;
namespace SyncVerse.Application.DTOs.AI.Meeting
{
    public class AiMeetingSummaryResponseDto
    {
        [JsonPropertyName("meeting_id")]
        public int MeetingId { get; set; }

        [JsonPropertyName("meeting_title")]
        public string MeetingTitle { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("key_points")]
        public List<string> KeyPoints { get; set; } = new();

        [JsonPropertyName("decisions")]
        public List<string> Decisions { get; set; } = new();

        [JsonPropertyName("risks")]
        public List<string> Risks { get; set; } = new();

        [JsonPropertyName("next_steps")]
        public List<string> NextSteps { get; set; } = new();

        [JsonPropertyName("action_items")]
        public List<AiActionItemDto> ActionItems { get; set; } = new();

        [JsonPropertyName("full_markdown")]
        public string FullMarkdown { get; set; } = string.Empty;
    }
}