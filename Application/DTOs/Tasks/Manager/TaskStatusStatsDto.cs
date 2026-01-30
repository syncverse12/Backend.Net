namespace Graduation_Project.Application.DTOs.Tasks.Manager
{
    public class TaskStatusStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Submitted { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
    }

}
