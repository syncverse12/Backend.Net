using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Comments;
using Graduation_Project.Application.Interfaces.Persistence;
using Graduation_Project.Application.Interfaces.Tasks.Comments;
using Graduation_Project.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Application.Services.Tasks.Comments
{
    public class TaskCommentService : ITaskCommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskCommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CommentResponseDto>> AddCommentAsync(string taskId, string userId, CreateCommentDto dto)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<CommentResponseDto>.Failure("Task not found");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                return Result<CommentResponseDto>.Failure("You don't have access to comment on this task");

            if (!string.IsNullOrEmpty(dto.ParentCommentId))
            {
                var parentComment = await _unitOfWork.Repository<TaskComment>()
                    .GetByIdAsync(dto.ParentCommentId);

                if (parentComment == null || parentComment.TaskId != taskId)
                    return Result<CommentResponseDto>.Failure("Parent comment not found");
            }

            var comment = new TaskComment
            {
                TaskId = taskId,
                UserId = userId,
                Content = dto.Content,
                ParentCommentId = dto.ParentCommentId
            };

            await _unitOfWork.Repository<TaskComment>().AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);

            var response = new CommentResponseDto
            {
                CommentId = comment.Id,
                TaskId = comment.TaskId,
                Content = comment.Content,
                UserId = comment.UserId,
                UserName = user?.UserName ?? "Unknown",
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId
            };

            return Result<CommentResponseDto>.Success(response, "Comment added successfully");
        }

        public async Task<Result<List<CommentResponseDto>>> GetTaskCommentsAsync(string taskId, string userId)
        {
            var task = await _unitOfWork.Repository<TaskItem>()
                .Query()
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (task == null)
                return Result<List<CommentResponseDto>>.Failure("Task not found");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                return Result<List<CommentResponseDto>>.Failure("You don't have access to view comments on this task");

            var comments = await _unitOfWork.Repository<TaskComment>()
                .Query()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.TaskId == taskId && !c.IsDeleted && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var commentDtos = comments.Select(c => MapToDto(c)).ToList();

            return Result<List<CommentResponseDto>>.Success(commentDtos);
        }

        public async Task<Result<CommentResponseDto>> UpdateCommentAsync(string commentId, string userId, UpdateCommentDto dto)
        {
            var comment = await _unitOfWork.Repository<TaskComment>()
                .Query()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
                return Result<CommentResponseDto>.Failure("Comment not found");

            if (comment.UserId != userId)
                return Result<CommentResponseDto>.Failure("You can only edit your own comments");

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<TaskComment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();

            var response = MapToDto(comment);

            return Result<CommentResponseDto>.Success(response, "Comment updated successfully");
        }

        public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId)
        {
            var comment = await _unitOfWork.Repository<TaskComment>()
                .Query()
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
                return Result<bool>.Failure("Comment not found");

            if (comment.UserId != userId)
                return Result<bool>.Failure("You can only delete your own comments");

            comment.IsDeleted = true;

            foreach (var reply in comment.Replies)
            {
                reply.IsDeleted = true;
            }

            _unitOfWork.Repository<TaskComment>().Update(comment);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Comment deleted successfully");
        }

        private CommentResponseDto MapToDto(TaskComment comment)
        {
            return new CommentResponseDto
            {
                CommentId = comment.Id,
                TaskId = comment.TaskId,
                Content = comment.Content,
                UserId = comment.UserId,
                UserName = comment.User?.UserName ?? "Unknown",
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = comment.Replies?
                    .Where(r => !r.IsDeleted)
                    .Select(r => MapToDto(r))
                    .ToList() ?? new List<CommentResponseDto>()
            };
        }
    }
}
