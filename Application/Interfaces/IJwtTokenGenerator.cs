using Domain.Entities;

namespace Application.Interfaces
{
    /// <summary>
    /// Interface for JWT token generation
    /// Application layer defines the contract
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        string GenerateToken(User user);
    }
}
