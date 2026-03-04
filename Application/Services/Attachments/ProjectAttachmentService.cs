using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Attachments;
using SyncVerse.Application.Interfaces.Attachments;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Storage;
using SyncVerse.Domain.Entities;

namespace SyncVerse.Application.Services.Attachments
{
    public class ProjectAttachmentService : IProjectAttachmentService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip", ".rar" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public ProjectAttachmentService(IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
        {
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(string projectId, IFormFile file, string userId)
        {
            // Validate project exists
            var project = await _unitOfWork.Repository<Domain.Entities.Project>().GetByIdAsync(projectId);
            if (project == null)
                return Result<AttachmentResponseDto>.Failure("Project not found");

            // Validate user is member
            var isMember = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);

            if (!isMember)
                return Result<AttachmentResponseDto>.Failure("You are not a member of this project");

            // Validate file
            if (file == null || file.Length == 0)
                return Result<AttachmentResponseDto>.Failure("File is empty");

            if (file.Length > MaxFileSize)
                return Result<AttachmentResponseDto>.Failure($"File size must not exceed {MaxFileSize / 1024 / 1024} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return Result<AttachmentResponseDto>.Failure($"File type {extension} is not allowed");

            // Upload file
            var folder = $"projects/{projectId}";
            string filePath;
            
            using (var stream = file.OpenReadStream())
            {
                filePath = await _fileStorageService.UploadFileAsync(stream, file.FileName, folder);
            }

            // Save to database
            var attachment = new ProjectAttachment
            {
                ProjectId = projectId,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<ProjectAttachment>().AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();

            var fileUrl = await _fileStorageService.GetFileUrlAsync(filePath);
            
            var user = await _unitOfWork.Repository<User>()
                .Query()
                .FirstOrDefaultAsync(u => u.Id == userId);

            return Result<AttachmentResponseDto>.Success(new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileUrl = fileUrl,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedByUserId = userId,
                UploadedByUserName = user?.UserName ?? "Unknown",
                UploadedAt = attachment.UploadedAt
            }, "File uploaded successfully");
        }

        public async Task<Result<List<AttachmentResponseDto>>> GetProjectAttachmentsAsync(string projectId)
        {
            var attachments = await _unitOfWork.Repository<ProjectAttachment>()
                .Query()
                .Include(a => a.UploadedByUser)
                .Where(a => a.ProjectId == projectId && !a.IsDeleted)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync();

            var dtos = new List<AttachmentResponseDto>();
            
            foreach (var attachment in attachments)
            {
                var fileUrl = await _fileStorageService.GetFileUrlAsync(attachment.FilePath);
                dtos.Add(new AttachmentResponseDto
                {
                    Id = attachment.Id,
                    FileName = attachment.FileName,
                    FileUrl = fileUrl,
                    ContentType = attachment.ContentType,
                    FileSize = attachment.FileSize,
                    UploadedByUserId = attachment.UploadedByUserId,
                    UploadedByUserName = attachment.UploadedByUser?.UserName ?? "Unknown",
                    UploadedAt = attachment.UploadedAt
                });
            }

            return Result<List<AttachmentResponseDto>>.Success(dtos);
        }

        public async Task<Result<bool>> DeleteAttachmentAsync(string attachmentId, string userId)
        {
            var attachment = await _unitOfWork.Repository<ProjectAttachment>()
                .Query()
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return Result<bool>.Failure("Attachment not found");

            // Check if user is project manager or the uploader
            var isManager = await _unitOfWork.Repository<ProjectMember>()
                .Query()
                .AnyAsync(m => m.ProjectId == attachment.ProjectId && 
                              m.UserId == userId && 
                              m.Role == Domain.Enums.ProjectRole.ProjectManager);

            if (attachment.UploadedByUserId != userId && !isManager)
                return Result<bool>.Failure("Unauthorized to delete this attachment");

            // Delete from storage
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // Soft delete from database
            attachment.IsDeleted = true;
            _unitOfWork.Repository<ProjectAttachment>().Update(attachment);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Attachment deleted successfully");
        }

        public async Task<Result<Stream>> DownloadAttachmentAsync(string attachmentId)
        {
            var attachment = await _unitOfWork.Repository<ProjectAttachment>()
                .Query()
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return Result<Stream>.Failure("Attachment not found");

            var stream = await _fileStorageService.DownloadFileAsync(attachment.FilePath);
            return Result<Stream>.Success(stream);
        }
    }
}
