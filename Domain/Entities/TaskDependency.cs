using SyncVerse.Domain.Common;

namespace SyncVerse.Domain.Entities
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
