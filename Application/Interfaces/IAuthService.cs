using Graduation_Project.Application.Common.Results;
using Graduation_Project.Application.DTOs.Auth;

namespace Graduation_Project.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
        Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    }
}