using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    /// <summary>
    /// Update user request - for system users only (Admin/Staff)
    /// Username cannot be changed after creation
    /// </summary>
    public record UpdateUserRequest
    {
        [Required(ErrorMessage = "Role is required")]
        [RegularExpression(@"^(Admin|Staff)$", ErrorMessage = "Role must be either 'Admin' or 'Staff'")]
        public string Role { get; init; } = "Staff";

        [Required(ErrorMessage = "Active status is required")]
        public bool IsActive { get; init; } = true;
    }
}
