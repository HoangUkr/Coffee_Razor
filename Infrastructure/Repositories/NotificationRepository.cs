using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly CoffeeDbContext _context;

        public NotificationRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<(IEnumerable<UserNotification> Notifications, int TotalCount)> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string scope)
        {
            var query = _context.UserNotifications
                .AsNoTracking()
                .Include(un => un.Notification)
                .Where(un => un.UserId == userId);

            var normalizedScope = string.IsNullOrWhiteSpace(scope) ? "today" : scope.Trim().ToLowerInvariant();
            var startOfToday = DateTimeOffset.UtcNow.Date;
            var startOfTomorrow = startOfToday.AddDays(1);

            if (normalizedScope == "today")
            {
                query = query.Where(un => un.Notification.CreatedDate >= startOfToday && un.Notification.CreatedDate < startOfTomorrow);
            }
            else if (normalizedScope == "past")
            {
                query = query.Where(un => un.Notification.CreatedDate < startOfToday);
            }

            var totalCount = await query.CountAsync();
            var notifications = await query
                .OrderByDescending(un => un.Notification.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notifications, totalCount);
        }

        public Task<int> GetUnreadCountAsync(Guid userId)
        {
            return _context.UserNotifications.CountAsync(un => un.UserId == userId && !un.IsRead);
        }

        public Task<UserNotification?> GetUserNotificationAsync(Guid userId, int notificationId)
        {
            return _context.UserNotifications
                .FirstOrDefaultAsync(un => un.UserId == userId && un.NotificationId == notificationId);
        }

        public async Task MarkAsReadAsync(UserNotification userNotification)
        {
            _context.UserNotifications.Update(userNotification);
            await _context.SaveChangesAsync();
        }
    }
}
