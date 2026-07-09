using System.Collections.Generic;

namespace SyncVerse.Application.DTOs.AI.ProjectPlanner
{
    public class AiProjectPlanRequestDto
    {
        public string Project_name { get; set; } = string.Empty;
        public string Deadline { get; set; } = string.Empty;
        public string Project_start { get; set; } = string.Empty;
        public int Sprint_length_days { get; set; } = 14;
        public int Hours_per_day { get; set; } = 8;
        public List<AiPlannerTaskDto> Tasks { get; set; } = new();
        public List<AiPlannerResourceDto> Resources { get; set; } = new();
    }

    public class AiPlannerTaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Estimated_hours { get; set; } = 0;
        public string Priority { get; set; } = "medium";
        public List<string> Required_skills { get; set; } = new();
        public List<string> Dependencies { get; set; } = new();
        public bool Is_milestone { get; set; } = false;
        public object? Metadata { get; set; }
    }

    public class AiPlannerResourceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Capacity { get; set; } = 1.0;
        public List<string> Skills { get; set; } = new();
        public string Available_from { get; set; } = string.Empty;
        public string Available_until { get; set; } = string.Empty;
    }
}
