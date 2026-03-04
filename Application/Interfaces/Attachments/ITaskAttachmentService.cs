using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Attachments;
using Microsoft.AspNetCore.Http;

namespace SyncVerse.Application.Interfaces.Attachments
{
    public interface ITaskAttachmentService
    {
        Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(string taskId, IFormFile file, string userId);
        Task<Result<List<AttachmentResponseDto>>> GetTaskAttachmentsAsync(string taskId);
        Task<Result<bool>> DeleteAttachmentAsync(string attachmentId, string userId);
        Task<Result<Stream>> DownloadAttachmentAsync(string attachmentId);
    }
}
