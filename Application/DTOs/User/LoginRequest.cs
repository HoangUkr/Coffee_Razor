using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public record LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; init; } = string.Empty;
    }
}
