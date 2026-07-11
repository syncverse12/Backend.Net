using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoWeeklySummaryResponseDto
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("period_start")]
        public DateTime PeriodStart { get; set; }

        [JsonPropertyName("period_end")]
        public DateTime PeriodEnd { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("memories_considered")]
        public int MemoriesConsidered { get; set; }

        [JsonPropertyName("highlighted_memories")]
        public List<EchoHighlightedMemoryDto> HighlightedMemories { get; set; } = new();
    }
}