using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using AutoMapper;
using Graduation_Project.API.JwtFeatuers;
using Graduation_Project.Application.DTOs.Auth;
using Graduation_Project.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Graduation_Project.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly JwtHandler jwtHandler;

        public UsersController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.jwtHandler = jwtHandler;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser(RegisterRequestDto userForRegisteration)
        {
            if (userForRegisteration is null)
                return BadRequest();

            var user = mapper.Map<User>(userForRegisteration);
            var result = await userManager.CreateAsync(user, userForRegisteration.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                return BadRequest(new RegisterResponseDto { Errors = errors });
            }
            return StatusCode(201);
        }

        [HttpPost("Log In")]
        public async Task<IActionResult> Authenticate(LoginRequestDto userForAuthentication)
        {
            var user = await userManager.FindByNameAsync(userForAuthentication.Email!);
            if (user is null || !await userManager.CheckPasswordAsync(user, userForAuthentication.Password))
                return Unauthorized(new AuthResponseDTO { ErrorMessage = "Invalid Authentication" });

            var token = jwtHandler.CreateToken(user);

            return Ok(new AuthResponseDTO { IsAuthSuccessful = true, Token = token });
        }
    }
}
