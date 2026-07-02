using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskRequestDto
    {
        [JsonPropertyName("project_name")]
        public string ProjectName { get; set; } = "SyncVerse Ecosystem";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "Virtual management platform with AI and VR capabilities.";

        [JsonPropertyName("client_name")]
        public string ClientName { get; set; } = "Internal Graduation Project";

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; } = "2026-01-01";

        [JsonPropertyName("deadline")]
        public string Deadline { get; set; } = "2026-07-15";

        [JsonPropertyName("estimated_hours")]
        public int EstimatedHours { get; set; } = 400;

        [JsonPropertyName("budget_usd")]
        public double BudgetUsd { get; set; } = 1500;

        [JsonPropertyName("team")]
        public List<TeamMemberRiskDto> Team { get; set; } = new();

        [JsonPropertyName("tech_stack")]
        public TechStackDto TechStack { get; set; } = new();

        [JsonPropertyName("required_skills")]
        public List<string> RequiredSkills { get; set; } = new();

        [JsonPropertyName("has_clear_requirements")]
        public bool HasClearRequirements { get; set; } = true;

        [JsonPropertyName("requirement_completeness_pct")]
        public int RequirementCompletenessPct { get; set; } = 85;

        [JsonPropertyName("similar_past_projects")]
        public List<string> SimilarPastProjects { get; set; } = new();

        [JsonPropertyName("dependencies_count")]
        public int DependenciesCount { get; set; } = 3;

        [JsonPropertyName("third_party_integrations_count")]
        public int ThirdPartyIntegrationsCount { get; set; } = 2;

        [JsonPropertyName("infrastructure_ready")]
        public bool InfrastructureReady { get; set; } = true;

        [JsonPropertyName("client_responsiveness")]
        public int ClientResponsiveness { get; set; } = 8;
    }
}