namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskRequestDto
    {
        public TechStackDto TechStack { get; set; } = new();
        public List<string> RequiredSkills { get; set; } = new();
        public bool HasClearRequirements { get; set; } = true;
        public int RequirementCompletenessPct { get; set; } = 80;
        public List<string> SimilarPastProjects { get; set; } = new();
        public int DependenciesCount { get; set; } = 0;
        public int ThirdPartyIntegrationsCount { get; set; } = 0;
        public bool InfrastructureReady { get; set; } = true;
        public int ClientResponsiveness { get; set; } = 7;
    }
}