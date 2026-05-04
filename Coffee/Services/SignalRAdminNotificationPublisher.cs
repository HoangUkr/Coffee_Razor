using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using WebUI.Hubs;

namespace WebUI.Services
{
    public class SignalRAdminNotificationPublisher : IAdminNotificationPublisher
    {
        private readonly IHubContext<AdminNotificationHub> _hubContext;

        public SignalRAdminNotificationPublisher(IHubContext<AdminNotificationHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public Task NotifyUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
        {
            var tasks = userIds
                .Distinct()
                .Select(userId => _hubContext.Clients.Group(AdminNotificationHub.GetUserGroup(userId)).SendAsync("NotificationReceived", cancellationToken));

            return Task.WhenAll(tasks);
        }
    }
}
