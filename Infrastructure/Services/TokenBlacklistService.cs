using System.IdentityModel.Tokens.Jwt;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private const string KeyPrefix = "auth:blacklist:";
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public TokenBlacklistService(IDistributedCache cache, IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task BlacklistTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || !_tokenHandler.CanReadToken(token))
            {
                return;
            }

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            var tokenId = jwtToken.Id;
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return;
            }

            var expiryUtc = jwtToken.ValidTo;
            var ttl = expiryUtc - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                ttl = TimeSpan.FromMinutes(1);
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetStringAsync(GetCacheKey(tokenId), "1", options, cancellationToken);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token) || !_tokenHandler.CanReadToken(token))
            {
                return false;
            }

            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return await IsTokenIdBlacklistedAsync(jwtToken.Id, cancellationToken);
        }

        public async Task<bool> IsTokenIdBlacklistedAsync(string tokenId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return false;
            }

            var cacheValue = await _cache.GetStringAsync(GetCacheKey(tokenId), cancellationToken);
            return !string.IsNullOrWhiteSpace(cacheValue);
        }

        private static string GetCacheKey(string tokenId) => $"{KeyPrefix}{tokenId}";
    }
}
