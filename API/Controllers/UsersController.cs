using Graduation_Project.Application.DTOs.Auth;
using Graduation_Project.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.IsAuthenticated ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return result.IsAuthenticated ? Ok(result) : Unauthorized(result);
        }
    }
}
