using Graduation_Project.Domain.Common;
using Synverse.Domain.Entities;

namespace Graduation_Project.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public decimal Budget { get; set; }
        //public ProjectStatus Status { get; set; }

        public ICollection<TaskEmployee> Tasks { get; set; }
            = new List<TaskEmployee>();
    }

}
