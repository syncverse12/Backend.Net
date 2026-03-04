namespace SyncVerse.Application.DTOs.Attachments
{
    public class AttachmentResponseDto
    {
        public string Id { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public string UploadedByUserId { get; set; } = null!;
        public string UploadedByUserName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
}
