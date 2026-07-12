using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoMemoryUploadDto
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("team_name")]
        public string TeamName { get; set; } = string.Empty;

        [JsonPropertyName("memory_type")]
        public string MemoryType { get; set; } = "financials"; 

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty; 

        [JsonPropertyName("author")]
        public string Author { get; set; } = "System_Auto_Collector";

        [JsonPropertyName("metadata")]
        public object Metadata { get; set; } = new { }; 
    }
}