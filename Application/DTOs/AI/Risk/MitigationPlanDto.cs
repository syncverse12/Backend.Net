namespace SyncVerse.Application.DTOs.AI.Risk
{
    public class MitigationPlanDto
    {
        public int Priority { get; set; }
        public string Action { get; set; } = string.Empty;
        public string OwnerRole { get; set; } = string.Empty;
        public string EstimatedImpact { get; set; } = string.Empty;
        public int TimeframeDays { get; set; }
    }
}
