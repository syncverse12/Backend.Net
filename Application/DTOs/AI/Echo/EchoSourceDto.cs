namespace SyncVerse.Application.DTOs.AI.Echo
{
    public class EchoSourceDto
    {
        public string MemoryId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string MemoryType { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
    }
}
