using System;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoTimelineItemDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("team_name")]
        public string TeamName { get; set; } = string.Empty;

        [JsonPropertyName("memory_type")]
        public string MemoryType { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}