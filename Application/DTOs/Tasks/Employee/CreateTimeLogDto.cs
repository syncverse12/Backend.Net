namespace Graduation_Project.Application.DTOs.Tasks.Employee
{
    public class CreateTimeLogDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
    }

}
