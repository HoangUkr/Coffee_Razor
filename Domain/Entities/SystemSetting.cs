using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class SystemSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; private set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Value { get; private set; } = string.Empty;

        public DateTimeOffset UpdatedAt { get; private set; }

        public SystemSetting(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty", nameof(key));

            Key = key.Trim();
            Value = value ?? string.Empty;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        private SystemSetting() { }

        public void SetValue(string value)
        {
            Value = value ?? string.Empty;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
