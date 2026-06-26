using System.Text.Json.Serialization;
namespace SyncVerse.Application.DTOs.AI.Meeting
{
    public class AiActionItemDto
    {
        [JsonPropertyName("task")]
        public string Task { get; set; } = string.Empty;

        [JsonPropertyName("owner")]
        public string Owner { get; set; } = string.Empty;

        [JsonPropertyName("deadline")]
        public string Deadline { get; set; } = string.Empty;
    }
}