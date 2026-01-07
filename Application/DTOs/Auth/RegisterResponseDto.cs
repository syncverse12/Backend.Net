namespace Graduation_Project.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public bool IsSuccessfulRegisteration {  get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
