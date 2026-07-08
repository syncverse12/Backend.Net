using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Interfaces.AI.Echo;

namespace SyncVerse.Api.Controllers.AI.Echo
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiBulkSyncController : ControllerBase
    {
        private readonly IAiBulkSyncService _bulkSyncService;

        public AiBulkSyncController(IAiBulkSyncService bulkSyncService)
        {
            _bulkSyncService = bulkSyncService;
        }

        [HttpPost("sync-project/{projectId}")]
        public async Task<IActionResult> SyncProjectData([FromRoute] Guid projectId)
        {
            var result = await _bulkSyncService.SyncAllApplicationDataToEchoAsync(projectId);

            if (result.IsSuccess)
            {
                return Ok(new { message = result.Message, recordsSynced = result.Data });
            }

            return BadRequest(result.Message);
        }
    }
}