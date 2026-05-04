using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    // Encapsulate the data returned after successful user operations
    public record UserResponse(
        Guid Id,
        string Username,
        string Role,
        bool IsActive,
        DateTimeOffset CreatedDate
    );
}
