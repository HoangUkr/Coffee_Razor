using Application.DTOs.Common;
using Application.DTOs.Notification;

namespace Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateForAdminsAsync(string where, string whatHappen, string? targetUrl = null);
        Task<PaginatedResult<NotificationResponse>> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string scope);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid userId, int notificationId);
    }
}
