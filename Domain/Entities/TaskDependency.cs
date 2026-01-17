using Graduation_Project.Domain.Common;

namespace Graduation_Project.Domain.Entities
{
    public class TaskDependency : BaseEntity
    {
        //Waiting Task
        public string TaskId { get; set; } = null!;
        public TaskItem Task { get; set; } = null!;

        // Depending Task
        public string DependsOnTaskId { get; set; } = null!;
        public TaskItem DependsOnTask { get; set; } = null!;
    }
}
