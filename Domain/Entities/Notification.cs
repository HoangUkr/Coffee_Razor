using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        [MaxLength(100)]
        public string Where { get; private set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string WhatHappen { get; private set; } = string.Empty;

        [MaxLength(500)]
        public string? TargetUrl { get; private set; }

        public DateTimeOffset CreatedDate { get; private set; }

        public virtual ICollection<UserNotification> UserNotifications { get; private set; } = new HashSet<UserNotification>();

        public Notification(string where, string whatHappen, string? targetUrl = null)
        {
            if (string.IsNullOrWhiteSpace(where))
                throw new ArgumentException("Notification source is required", nameof(where));
            if (string.IsNullOrWhiteSpace(whatHappen))
                throw new ArgumentException("Notification message is required", nameof(whatHappen));

            Where = where.Trim();
            WhatHappen = whatHappen.Trim();
            TargetUrl = string.IsNullOrWhiteSpace(targetUrl) ? null : targetUrl.Trim();
            CreatedDate = DateTimeOffset.UtcNow;
        }

        private Notification()
        {
        }
    }
}
