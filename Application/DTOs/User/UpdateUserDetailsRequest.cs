using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public record UpdateUserDetailsRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; init; } = "Staff"; // "Admin" or "Staff"
    }
}
