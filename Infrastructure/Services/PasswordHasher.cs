using Application.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 4;
        private const int MemorySize = 65536;
        private const int Parallelism = 1;

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("Password cannot be null or empty");
            }

            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            byte[] hash = HashPasswordWithSalt(password, salt);
            return (hash, salt);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }
            if (storedHash == null || storedSalt == null)
            {
                return false;
            }

            byte[] hashToCompare = HashPasswordWithSalt(password, storedSalt);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, storedHash);
        }

        private byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                Iterations = Iterations,
                MemorySize = MemorySize,
                DegreeOfParallelism = Parallelism
            };
            return argon2.GetBytes(HashSize);
        }
    }
}
