namespace Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing operations
    /// Application layer defines the contract
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a password and returns hash and salt
        /// </summary>
        (byte[] hash, byte[] salt) HashPassword(string password);
        
        /// <summary>
        /// Verifies a password against stored hash and salt
        /// </summary>
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
    }
}
 