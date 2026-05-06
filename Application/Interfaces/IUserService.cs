using Application.DTOs.User;
using Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserResponse?> GetByIdAsync(Guid id);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<UserResponse?> UpdateUsernameAsync(Guid userId, UpdateUserRequest request);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<bool> DeactivateAccountAsync(Guid userId);

        // Admin methods
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<PaginatedResult<UserResponse>> SearchAsync(SearchParameters parameters, bool includeInactive = false);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse?> UpdateUserDetailsAsync(Guid userId, UpdateUserDetailsRequest request);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<bool> ActivateUserAsync(Guid userId);
    }
}
