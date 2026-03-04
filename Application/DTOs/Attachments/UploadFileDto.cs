using Microsoft.AspNetCore.Http;

namespace SyncVerse.Application.DTOs.Attachments
{
    public class UploadFileDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
