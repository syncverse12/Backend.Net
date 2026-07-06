namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskProfileEnrichmentDto
    {
        public int EstimatedHours { get; set; } 
        public int ClientResponsiveness { get; set; } 
        public int ThirdPartyIntegrationsCount { get; set; } 
        public List<string> SimilarPastProjects { get; set; } = new(); 
    }
}
