using Application.DTOs.Common;
using Application.DTOs.Notification;
using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAdminNotificationPublisher _notificationPublisher;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(IAdminNotificationPublisher notificationPublisher, INotificationRepository notificationRepository, IUserRepository userRepository)
        {
            _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task CreateForAdminsAsync(string where, string whatHappen, string? targetUrl = null)
        {
            var admins = (await _userRepository.GetAllActiveAsync())
                .Where(u => string.Equals(u.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!admins.Any())
            {
                return;
            }

            var notification = new Notification(where, whatHappen, targetUrl);
            foreach (var admin in admins)
            {
                notification.UserNotifications.Add(new UserNotification(admin.Id));
            }

            await _notificationRepository.CreateAsync(notification);
            await _notificationPublisher.NotifyUsersAsync(admins.Select(a => a.Id));
        }

        public async Task<PaginatedResult<NotificationResponse>> GetForUserAsync(Guid userId, int pageNumber, int pageSize, string scope)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(userId));

            var (notifications, totalCount) = await _notificationRepository.GetForUserAsync(userId, pageNumber, pageSize, scope);
            var responses = notifications.Select(n => new NotificationResponse
            {
                Id = n.NotificationId,
                Where = n.Notification.Where,
                WhatHappen = n.Notification.WhatHappen,
                TargetUrl = n.Notification.TargetUrl,
                IsRead = n.IsRead,
                CreatedDate = n.Notification.CreatedDate
            }).ToList();

            return new PaginatedResult<NotificationResponse>(responses, totalCount, pageNumber, pageSize);
        }

        public Task<int> GetUnreadCountAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(userId));

            return _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, int notificationId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID is required", nameof(userId));
            if (notificationId <= 0)
                throw new ArgumentException("Notification ID must be greater than 0", nameof(notificationId));

            var userNotification = await _notificationRepository.GetUserNotificationAsync(userId, notificationId);
            if (userNotification == null)
            {
                return false;
            }

            userNotification.MarkAsRead();
            await _notificationRepository.MarkAsReadAsync(userNotification);
            return true;
        }
    }
}
