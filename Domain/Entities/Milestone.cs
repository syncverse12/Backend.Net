using Graduation_Project.Domain.Common;

namespace Graduation_Project.Domain.Entities
{
    public class Milestone : BaseEntity
    {
        public string ProjectId { get; set; } = null!;
        public Project Project { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
