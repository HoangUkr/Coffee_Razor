using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebUI.Hubs
{
    [Authorize]
    public class AdminNotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdValue, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdValue, out var userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroup(userId));
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetUserGroup(Guid userId) => $"admin-notifications:{userId}";
    }
}
