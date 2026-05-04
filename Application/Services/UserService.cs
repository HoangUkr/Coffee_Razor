using Application.Interfaces;
using Application.Repositories;
using Application.DTOs.User;
using Application.DTOs.Common;
using Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if(await _userRepository.ExistsAsync(request.Username))
            {
                throw new InvalidOperationException($"Username {request.Username} already exists.");
            }

            var (hash, salt) = _passwordHasher.HashPassword(request.Password);
            var user = new User(request.Username, hash, salt);
            await _userRepository.CreateAsync(user);
            
            return _mapper.Map<UserResponse>(user);
        }
        public async Task<UserResponse?> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserResponse>(user) : null;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _userRepository.ExistsAsync(username);
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is deactivated");
            }

            bool isPasswordValid = _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            string token = _jwtTokenGenerator.GenerateToken(user);
            var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
            var expiry = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

            var userResponse = _mapper.Map<UserResponse>(user);
            return new LoginResponse(token, expiry, userResponse);
        }

        public async Task<UserResponse?> UpdateUsernameAsync(Guid userId, UpdateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException("Cannot update inactive account");
            }

            // Update role
            user.UpdateRole(request.Role);

            // Update active status
            if (request.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            // Save changes
            await _userRepository.UpdateAsync(user);

            // Return updated user
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException("Cannot update password for inactive account");
            }

            // Verify current password
            bool isCurrentPasswordValid = _passwordHasher.VerifyPassword(
                request.CurrentPassword,
                user.PasswordHash,
                user.PasswordSalt
            );

            if (!isCurrentPasswordValid)
            {
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // Hash new password
            var (newHash, newSalt) = _passwordHasher.HashPassword(request.NewPassword);

            // Update password
            user.UpdatePassword(newHash, newSalt);

            // Save changes
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> DeactivateAccountAsync(Guid userId)
        {
            // Get existing user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!user.IsActive)
            {
                // Already deactivated
                return true;
            }

            // Deactivate account
            user.Deactivate();

            // Save changes
            await _userRepository.UpdateAsync(user);

            return true;
        }

        // Admin methods
        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if username already exists
            if (await _userRepository.ExistsAsync(request.Username))
            {
                throw new InvalidOperationException($"Username '{request.Username}' already exists");
            }

            // Validate role
            if (request.Role != "Admin" && request.Role != "Staff")
            {
                throw new ArgumentException("Role must be either 'Admin' or 'Staff'", nameof(request.Role));
            }

            // Hash password
            var (hash, salt) = _passwordHasher.HashPassword(request.Password);

            // Create user
            var user = new User(request.Username, hash, salt, request.Role);
            await _userRepository.CreateAsync(user);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse?> UpdateUserDetailsAsync(Guid userId, UpdateUserDetailsRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Check if new username conflicts with existing user
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null && existingUser.Id != userId)
            {
                throw new InvalidOperationException($"Username '{request.Username}' is already taken");
            }

            // Update role
            user.UpdateRole(request.Role);

            // Save changes
            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (user.IsActive)
            {
                return true; // Already active
            }

            user.Activate();
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<PaginatedResult<UserResponse>> SearchAsync(SearchParameters parameters, bool includeInactive = false)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var (users, totalCount) = await _userRepository.SearchAsync(
                parameters.SearchTerm,
                parameters.PageNumber,
                parameters.PageSize,
                includeInactive);

            var userResponses = _mapper.Map<IEnumerable<UserResponse>>(users);

            return new PaginatedResult<UserResponse>(
                userResponses,
                totalCount,
                parameters.PageNumber,
                parameters.PageSize);
        }
    }
}
