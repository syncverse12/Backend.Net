using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Risk;
using SyncVerse.Application.Interfaces.AI;
using SyncVerse.Application.Interfaces.AI.Risk;
using System.Threading.Tasks;

namespace SyncVerse.API.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class AiRiskController : ControllerBase
    {
        private readonly IAiRiskService _aiRiskService;

        public AiRiskController(IAiRiskService aiRiskService)
        {
            _aiRiskService = aiRiskService;
        }

        [HttpPost("analyze-project")]
        public async Task<IActionResult> AnalyzeProject([FromBody] ProjectRiskRequestDto dto)
        {
            var result = await _aiRiskService.AnalyzeProjectRisksAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("live-update")]
        public async Task<IActionResult> UpdateLiveRisks([FromBody] LiveRiskUpdateRequestDto dto)
        {
            var result = await _aiRiskService.UpdateLiveRisksAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }
    }
}