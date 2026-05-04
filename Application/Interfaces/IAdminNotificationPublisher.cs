namespace Application.Interfaces
{
    public interface IAdminNotificationPublisher
    {
        Task NotifyUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
