using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.Attachments;
using SyncVerse.Application.Interfaces.Attachments;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers.Attachments
{
    [ApiController]
    [Route("api/task-attachments")]
    [Authorize]
    public class TaskAttachmentsController : ControllerBase
    {
        private readonly ITaskAttachmentService _attachmentService;
        private readonly ICurrentUserService _currentUserService;

        public TaskAttachmentsController(
            ITaskAttachmentService attachmentService,
            ICurrentUserService currentUserService)
        {
            _attachmentService = attachmentService;
            _currentUserService = currentUserService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Result<AttachmentResponseDto>>> UploadAttachment(
            [FromForm] string taskId,
            IFormFile file)
        {
            var userId = _currentUserService.UserId!;
            var result = await _attachmentService.UploadAttachmentAsync(taskId, file, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{taskId}")]
        public async Task<ActionResult<Result<List<AttachmentResponseDto>>>> GetAttachments(string taskId)
        {
            var result = await _attachmentService.GetTaskAttachmentsAsync(taskId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{attachmentId}")]
        public async Task<ActionResult<Result<bool>>> DeleteAttachment(string attachmentId)
        {
            var userId = _currentUserService.UserId!;
            var result = await _attachmentService.DeleteAttachmentAsync(attachmentId, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("download/{attachmentId}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadAttachment(string attachmentId)
        {
            var result = await _attachmentService.DownloadAttachmentAsync(attachmentId);

            if (!result.IsSuccess || result.Data == null)
                return BadRequest(new { message = result.Message });

            return File(result.Data, "application/octet-stream", enableRangeProcessing: true);
        }
    }
}
