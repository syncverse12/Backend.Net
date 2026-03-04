using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Attachments;
using Microsoft.AspNetCore.Http;

namespace SyncVerse.Application.Interfaces.Attachments
{
    public interface IProjectAttachmentService
    {
        Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(string projectId, IFormFile file, string userId);
        Task<Result<List<AttachmentResponseDto>>> GetProjectAttachmentsAsync(string projectId);
        Task<Result<bool>> DeleteAttachmentAsync(string attachmentId, string userId);
        Task<Result<Stream>> DownloadAttachmentAsync(string attachmentId);
    }
}
