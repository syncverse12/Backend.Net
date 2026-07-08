using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoSourceDto
    {
        [JsonPropertyName("memory_id")]
        public string MemoryId { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty; 

        [JsonPropertyName("memory_type")]
        public string MemoryType { get; set; } = string.Empty;

        [JsonPropertyName("team_name")]
        public string TeamName { get; set; } = string.Empty;

        [JsonPropertyName("relevance_score")]
        public double RelevanceScore { get; set; }
    }
}
