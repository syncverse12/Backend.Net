using SyncVerse.Domain.Common;

namespace SyncVerse.Domain.Entities
{
    public class ProjectAttachment : BaseEntity
    {
        public string ProjectId { get; set; } = null!;
        public Project Project { get; set; } = null!;
        
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        
        public string UploadedByUserId { get; set; } = null!;
        public User UploadedByUser { get; set; } = null!;
        
        public DateTime UploadedAt { get; set; }
    }
}
