namespace Application.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task BlacklistTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> IsTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> IsTokenIdBlacklistedAsync(string tokenId, CancellationToken cancellationToken = default);
    }
}
