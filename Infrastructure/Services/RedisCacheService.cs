using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(cachedValue))
            {
                _logger.LogInformation("CACHE MISS | Backend: {CacheBackend} | Key: {CacheKey}", _distributedCache.GetType().Name, key);
                return default;
            }

            _logger.LogInformation("CACHE HIT | Backend: {CacheBackend} | Key: {CacheKey}", _distributedCache.GetType().Name, key);

            return JsonSerializer.Deserialize<T>(cachedValue, SerializerOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var serializedValue = JsonSerializer.Serialize(value, SerializerOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
            _logger.LogInformation("CACHE SET | Backend: {CacheBackend} | Key: {CacheKey} | TTL: {TtlMinutes} minutes", _distributedCache.GetType().Name, key, expiration.TotalMinutes);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("CACHE REMOVE | Backend: {CacheBackend} | Key: {CacheKey}", _distributedCache.GetType().Name, key);
            return _distributedCache.RemoveAsync(key, cancellationToken);
        }

        public Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            var keyList = keys.ToList();
            _logger.LogInformation("CACHE REMOVE MANY | Backend: {CacheBackend} | Keys: {CacheKeys}", _distributedCache.GetType().Name, string.Join(", ", keyList));
            return Task.WhenAll(keyList.Select(key => _distributedCache.RemoveAsync(key, cancellationToken)));
        }
    }
}
