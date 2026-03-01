using SyncVerse.Application.DTOs.Tasks.Comments;
using SyncVerse.Application.Interfaces.Tasks.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SyncVerse.API.Controllers.Tasks.Employee
{
    [Authorize]
    [ApiController]
    [Route("api/tasks/{taskId}/comments")]
    public class TaskCommentsController : ControllerBase
    {
        private readonly ITaskCommentService _commentService;

        public TaskCommentsController(ITaskCommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string taskId, [FromBody] CreateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _commentService.AddCommentAsync(taskId, userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(string taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _commentService.GetTaskCommentsAsync(taskId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(string commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _commentService.UpdateCommentAsync(commentId, userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _commentService.DeleteCommentAsync(commentId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
