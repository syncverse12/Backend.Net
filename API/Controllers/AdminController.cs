using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok("----Welcome Admin----");
        }
    }
}
