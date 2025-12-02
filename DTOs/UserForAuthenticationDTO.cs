using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Graduation_Project.DTOs
{
    public class UserForAuthenticationDTO
    {
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
