namespace Graduation_Project.Application.DTOs.Tasks.Comments
{
    public class CommentResponseDto
    {
        public string CommentId { get; set; } = null!;
        public string TaskId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ParentCommentId { get; set; }
        public List<CommentResponseDto> Replies { get; set; } = new();
    }
}
