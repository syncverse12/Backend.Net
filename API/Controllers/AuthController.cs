using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SyncVerse.Application.Interfaces.WorkspaceInvitation;
using SyncVerse.Application.DTOs.WorkspaceInvitation;
using SyncVerse.Application.DTOs.Auth;
using SyncVerse.Application.Interfaces.Identity;

namespace SyncVerse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register new user (sends OTP to email)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify email with OTP
        /// </summary>
        [HttpPost("verify-email/{userId}")]
        public async Task<IActionResult> VerifyEmail(string userId, [FromBody] VerifyEmailDto dto)
        {
            var result = await _authService.VerifyEmailAsync(dto, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Login (requires verified email)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Forgot password (sends OTP to email)
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Verify forgot password OTP
        /// </summary>
        [HttpPost("verify-reset-otp/{userId}")]
        public async Task<IActionResult> VerifyResetOtp(string userId, [FromBody] VerifyOtpDto dto)
        {
            var result = await _authService.VerifyForgotPasswordOtpAsync(dto, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Reset password after OTP verification
        /// </summary>
        [HttpPost("reset-password/{userId}")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto, userId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Resend verification OTP
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ResendVerificationOtpAsync(dto.Email);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}