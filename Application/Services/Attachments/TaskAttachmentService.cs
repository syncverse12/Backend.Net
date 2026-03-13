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
    public class TaskAttachmentService : ITaskAttachmentService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip", ".rar", ".txt" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public TaskAttachmentService(IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
        {
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AttachmentResponseDto>> UploadAttachmentAsync(string taskId, IFormFile file, string userId)
        {
            // Validate task exists
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<AttachmentResponseDto>.Failure("Task not found");

            // Validate user can upload (all project members regardless of role)
            if (!string.IsNullOrWhiteSpace(task.ProjectId))
            {
                var isProjectMember = await _unitOfWork.Repository<ProjectMember>()
                    .Query()
                    .AnyAsync(m => m.ProjectId == task.ProjectId && m.UserId == userId && m.IsActive);

                if (!isProjectMember)
                    return Result<AttachmentResponseDto>.Failure("You are not a member of this project");
            }
            else if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
            {
                return Result<AttachmentResponseDto>.Failure("You are not authorized to upload files to this task");
            }

            // Validate file
            if (file == null || file.Length == 0)
                return Result<AttachmentResponseDto>.Failure("File is empty");

            if (file.Length > MaxFileSize)
                return Result<AttachmentResponseDto>.Failure($"File size must not exceed {MaxFileSize / 1024 / 1024} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return Result<AttachmentResponseDto>.Failure($"File type {extension} is not allowed");

            // Upload file
            var folder = $"tasks/{taskId}";
            string filePath;
            
            using (var stream = file.OpenReadStream())
            {
                filePath = await _fileStorageService.UploadFileAsync(stream, file.FileName, folder);
            }

            // Save to database
            var attachment = new TaskAttachment
            {
                TaskId = taskId,
                FileName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<TaskAttachment>().AddAsync(attachment);
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

        public async Task<Result<List<AttachmentResponseDto>>> GetTaskAttachmentsAsync(string taskId)
        {
            var attachments = await _unitOfWork.Repository<TaskAttachment>()
                .Query()
                .Include(a => a.UploadedByUser)
                .Where(a => a.TaskId == taskId && !a.IsDeleted)
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
            var attachment = await _unitOfWork.Repository<TaskAttachment>()
                .Query()
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return Result<bool>.Failure("Attachment not found");

            // Allow delete for all project members regardless of role
            if (!string.IsNullOrWhiteSpace(attachment.Task.ProjectId))
            {
                var isProjectMember = await _unitOfWork.Repository<ProjectMember>()
                    .Query()
                    .AnyAsync(m => m.ProjectId == attachment.Task.ProjectId && m.UserId == userId && m.IsActive);

                if (!isProjectMember)
                    return Result<bool>.Failure("Unauthorized to delete this attachment");
            }
            else if (attachment.UploadedByUserId != userId &&
                     attachment.Task.CreatedByUserId != userId &&
                     attachment.Task.AssignedToUserId != userId)
            {
                return Result<bool>.Failure("Unauthorized to delete this attachment");
            }

            // Delete from storage
            await _fileStorageService.DeleteFileAsync(attachment.FilePath);

            // Soft delete from database
            attachment.IsDeleted = true;
            _unitOfWork.Repository<TaskAttachment>().Update(attachment);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Attachment deleted successfully");
        }

        public async Task<Result<Stream>> DownloadAttachmentAsync(string attachmentId)
        {
            var attachment = await _unitOfWork.Repository<TaskAttachment>()
                .Query()
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDeleted);

            if (attachment == null)
                return Result<Stream>.Failure("Attachment not found");

            var stream = await _fileStorageService.DownloadFileAsync(attachment.FilePath);
            return Result<Stream>.Success(stream);
        }
    }
}
