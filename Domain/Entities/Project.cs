using Synverse.Domain.Entities;

namespace Graduation_Project.Domain.Entities
{
    public class Project
    {
        public ICollection<TaskEmployee> Tasks { get; set; }
            = new List<TaskEmployee>();
    }
}
