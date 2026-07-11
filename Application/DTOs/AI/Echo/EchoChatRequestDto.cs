using System;
using System.Text.Json.Serialization; 

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoChatRequestDto
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}