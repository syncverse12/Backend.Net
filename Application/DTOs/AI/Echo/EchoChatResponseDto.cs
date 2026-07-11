using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoChatResponseDto
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("sources")]
        public List<EchoSourceDto> Sources { get; set; } = new();

        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}