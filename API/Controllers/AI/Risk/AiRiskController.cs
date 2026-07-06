using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.DTOs.AI.Risk;
using SyncVerse.Application.Interfaces.AI.Risk;
using System;
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

        [HttpPut("project-profile/{projectId}")]
        public async Task<IActionResult> SaveProjectProfile(Guid projectId, [FromBody] ProjectRiskProfileEnrichmentDto enrichmentData)
        {
            var result = await _aiRiskService.SaveProjectRiskProfileAsync(projectId, enrichmentData);

            if (result.IsSuccess)
            {
                return Ok(new { message = result.Message });
            }

            return BadRequest(result.Message);
        }

        [HttpPost("analyze-project/{projectId}")]
        public async Task<IActionResult> AnalyzeProject(Guid projectId)
        {
            var result = await _aiRiskService.AnalyzeProjectRisksAsync(projectId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost("live-update")]
        public async Task<IActionResult> UpdateLiveMetrics([FromBody] LiveRiskUpdateRequestDto dto)
        {
            var result = await _aiRiskService.UpdateLiveRisksAsync(dto);

            if (result.IsSuccess)
            {
                return Ok(result.Data); 
            }

            return BadRequest(result.Message);
        }
    }
}