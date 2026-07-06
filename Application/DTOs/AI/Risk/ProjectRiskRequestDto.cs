using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class ProjectRiskRequestDto
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty; 
        public string ProjectName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public int EstimatedHours { get; set; }
        public double BudgetUsd { get; set; }
        public List<TeamMemberDto> Team { get; set; } = new();
        public TechStackDto TechStack { get; set; } = new();
        public List<string> RequiredSkills { get; set; } = new();
        public bool HasClearRequirements { get; set; }
        public double RequirementCompletenessPct { get; set; }
        public List<string> SimilarPastProjects { get; set; } = new();
        public int DependenciesCount { get; set; }
        public int ThirdPartyIntegrationsCount { get; set; }
        public bool InfrastructureReady { get; set; }
        public int ClientResponsiveness { get; set; }
    }
}