using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Tasks.Comments;

namespace Graduation_Project.Application.Interfaces.Tasks.Comments
{
    public interface ITaskCommentService
    {
        Task<Result<CommentResponseDto>> AddCommentAsync(string taskId, string userId, CreateCommentDto dto);
        Task<Result<List<CommentResponseDto>>> GetTaskCommentsAsync(string taskId, string userId);
        Task<Result<CommentResponseDto>> UpdateCommentAsync(string commentId, string userId, UpdateCommentDto dto);
        Task<Result<bool>> DeleteCommentAsync(string commentId, string userId);
    }
}
