using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public record CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; init; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; init; } = "Staff"; // "Admin" or "Staff"
    }
}
