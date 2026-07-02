using System.Text.Json.Serialization;

public class TeamMemberRiskDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Member";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "Developer";

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = new();

    [JsonPropertyName("current_workload_pct")]
    public int CurrentWorkloadPct { get; set; } = 50;

    [JsonPropertyName("seniority_years")]
    public int SeniorityYears { get; set; } = 1;
}