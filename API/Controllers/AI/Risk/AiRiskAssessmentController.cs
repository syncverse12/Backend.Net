using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncVerse.Application.Interfaces.AI.Risk;

namespace SyncVerse.API.Controllers.AI.Risk
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiRiskAssessmentController : ControllerBase
    {
        private readonly IAiRiskAssessmentService _aiRiskAssessmentService;

        public AiRiskAssessmentController(IAiRiskAssessmentService aiRiskAssessmentService)
        {
            _aiRiskAssessmentService = aiRiskAssessmentService;
        }

        [HttpPost("analyze/{projectId}")]
        public async Task<IActionResult> AnalyzeProject([FromRoute] string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
                return BadRequest("Project ID is required.");

            var result = await _aiRiskAssessmentService.AnalyzeProjectRisksAsync(projectId);

            if (result.IsSuccess)
                return Ok(result.Data);

            return BadRequest(result.Message);
        }
    }
}