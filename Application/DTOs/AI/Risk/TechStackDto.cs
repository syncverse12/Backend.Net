namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class TechStackDto
    {
        public List<string> Languages { get; set; } = new();
        public List<string> Frameworks { get; set; } = new();
        public List<string> Infrastructure { get; set; } = new();
        public List<string> ThirdPartyApis { get; set; } = new();
    }
}
