using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Interfaces.AI.Risk;


namespace SyncVerse.Api.Controllers.AI
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiRiskController : ControllerBase
    {
        private readonly IAiRiskService _riskService;

        public AiRiskController(IAiRiskService riskService)
        {
            _riskService = riskService;
        }

        [HttpPost("project/{projectId}/analyze-risks")]
        public async Task<IActionResult> AnalyzeProjectRisks([FromRoute] Guid projectId)
        {
            var analysisResult = await _riskService.AnalyzeProjectRisksAsync(projectId);
            return Ok(analysisResult);
        }

        [HttpGet("project/{projectId}/risk-history")]
        public async Task<IActionResult> GetProjectRiskHistory([FromRoute] Guid projectId, [FromQuery] int limit = 20)
        {
            if (projectId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid Project ID." });
            }

            var historyResult = await _riskService.GetProjectRiskHistoryAsync(projectId, limit);
            return Ok(historyResult);
        }
    }
}