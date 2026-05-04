using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public record LoginResponse(string Token, DateTimeOffset Expiry, UserResponse User);
}
