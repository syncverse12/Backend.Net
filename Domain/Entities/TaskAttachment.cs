using SyncVerse.Domain.Common;

namespace SyncVerse.Domain.Entities
{
    public class TaskAttachment : BaseEntity
    {
        public string TaskId { get; set; } = null!;
        public TaskItem Task { get; set; } = null!;
        
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        
        public string UploadedByUserId { get; set; } = null!;
        public User UploadedByUser { get; set; } = null!;
        
        public DateTime UploadedAt { get; set; }
    }
}
