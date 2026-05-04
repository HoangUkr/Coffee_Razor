using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserNotification
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public Guid UserId { get; private set; }

        [Required]
        public int NotificationId { get; private set; }

        public bool IsRead { get; private set; }
        public DateTimeOffset? ReadDate { get; private set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; private set; } = null!;

        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; private set; } = null!;

        public UserNotification(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(userId));

            UserId = userId;
            IsRead = false;
        }

        private UserNotification()
        {
        }

        public void MarkAsRead()
        {
            if (IsRead)
            {
                return;
            }

            IsRead = true;
            ReadDate = DateTimeOffset.UtcNow;
        }
    }
}
