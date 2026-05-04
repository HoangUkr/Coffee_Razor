using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    /// <summary>
    /// System user entity for admin and staff only
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; private set; }

        [Required]
        public byte[] PasswordHash { get; private set; }

        [Required]
        public byte[] PasswordSalt { get; private set; }

        [Required]
        public int PasswordVersion { get; private set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; private set; } // "Admin" or "Staff"

        public bool IsActive { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }

        public User(string username, byte[] hash, byte[] salt, string role = "Staff")
        {
            Id = Guid.NewGuid();
            Username = username ?? throw new ArgumentNullException(nameof(username));
            PasswordHash = hash ?? throw new ArgumentNullException(nameof(hash));
            PasswordSalt = salt ?? throw new ArgumentNullException(nameof(salt));

            if (string.IsNullOrWhiteSpace(role) || (role != "Admin" && role != "Staff"))
                throw new ArgumentException("Role must be either 'Admin' or 'Staff'", nameof(role));

            Role = role;
            PasswordVersion = 1;
            IsActive = true;
            CreatedDate = DateTimeOffset.UtcNow;
        }

        private User() { }

        public void UpdatePassword(byte[] newHash, byte[] newSalt)
        {
            PasswordHash = newHash ?? throw new ArgumentNullException(nameof(newHash));
            PasswordSalt = newSalt ?? throw new ArgumentNullException(nameof(newSalt));
            PasswordVersion++;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateRole(string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole) || (newRole != "Admin" && newRole != "Staff"))
                throw new ArgumentException("Role must be either 'Admin' or 'Staff'", nameof(newRole));

            Role = newRole;
        }
    }
}
