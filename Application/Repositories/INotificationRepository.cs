using Domain.Entities;

namespace Application.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<(IEnumerable<UserNotification> Notifications, int TotalCount)> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string scope);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<UserNotification?> GetUserNotificationAsync(Guid userId, int notificationId);
        Task MarkAsReadAsync(UserNotification userNotification);
    }
}
