using System.Text.Json.Serialization;

public class TeamMemberDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Skills { get; set; } = new();
    public double CurrentWorkloadPct { get; set; }
    public int SeniorityYears { get; set; }
}