namespace Graduation_Project.Application.DTOs.Tasks.Comments
{
    public class CreateCommentDto
    {
        public string Content { get; set; } = null!;
        public string? ParentCommentId { get; set; }
    }
}
