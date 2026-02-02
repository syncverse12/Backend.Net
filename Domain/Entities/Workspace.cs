using Graduation_Project.Domain.Common;
using Graduation_Project.Domain.Entities;

namespace Graduation_Project.Domain.Entities
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CreatedByUserId { get; set; } = null!;
        public User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
