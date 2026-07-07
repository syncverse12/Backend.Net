using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Echo;
using SyncVerse.Application.Interfaces.AI.Echo;

namespace SyncVerse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiEchoController : ControllerBase
    {
        private readonly IAiEchoService _echoService;

        public AiEchoController(IAiEchoService echoService)
        {
            _echoService = echoService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ChatWithEcho([FromBody] EchoChatRequestDto dto)
        {
            var result = await _echoService.TalkToEchoAsync(dto);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }
    }
}