using System.Collections.Generic;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public List<EchoSourceDto> Sources { get; set; } = new();
        public string Mode { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }
}