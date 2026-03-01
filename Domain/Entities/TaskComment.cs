using SyncVerse.Domain.Common;

namespace SyncVerse.Domain.Entities
{
    public class TaskComment : BaseEntity
    {
        public string TaskId { get; set; } = null!;
        public TaskItem Task { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string Content { get; set; } = null!;

        public string? ParentCommentId { get; set; }
        public TaskComment? ParentComment { get; set; }

        public ICollection<TaskComment> Replies { get; set; } = new List<TaskComment>();
    }
}
