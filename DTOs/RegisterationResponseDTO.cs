namespace Graduation_Project.DTOs
{
    public class RegisterationResponseDTO
    {
        public bool IsSuccessfulRegisteration {  get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
