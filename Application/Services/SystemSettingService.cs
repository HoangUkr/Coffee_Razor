using Application.DTOs.Settings;
using Application.Interfaces;
using Application.Repositories;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private const string CacheKey = "system:settings";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

        private readonly ISystemSettingRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<SystemSettingService> _logger;

        public SystemSettingService(
            ISystemSettingRepository repository,
            ICacheService cacheService,
            ILogger<SystemSettingService> logger)
        {
            _repository  = repository  ?? throw new ArgumentNullException(nameof(repository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            var cached = await _cacheService.GetAsync<AppSettings>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("SETTINGS SOURCE | Source: CACHE");
                return cached;
            }

            var rows = await _repository.GetAllAsync();
            var map  = rows.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            var settings = new AppSettings
            {
                ContactEmail      = Get(map, SettingKeys.ContactEmail,      string.Empty),
                ContactPhone      = Get(map, SettingKeys.ContactPhone,      string.Empty),
                ContactAddress    = Get(map, SettingKeys.ContactAddress,    string.Empty),
                ContactFacebook   = Get(map, SettingKeys.ContactFacebook,   string.Empty),
                ContactInstagram  = Get(map, SettingKeys.ContactInstagram,  string.Empty),
                ContactTwitter    = Get(map, SettingKeys.ContactTwitter,    string.Empty),
                EmailConfirmationEnabled = GetBool(map, SettingKeys.EmailConfirmationEnabled, defaultValue: true),
                ShowNotificationCount    = GetBool(map, SettingKeys.ShowNotificationCount,    defaultValue: true),
            };

            await _cacheService.SetAsync(CacheKey, settings, CacheDuration);
            _logger.LogInformation("SETTINGS SOURCE | Source: DB");
            return settings;
        }

        public async Task UpdateAsync(UpdateSettingsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var updated = new[]
            {
                new SystemSetting(SettingKeys.ContactEmail,              request.ContactEmail     ?? string.Empty),
                new SystemSetting(SettingKeys.ContactPhone,              request.ContactPhone     ?? string.Empty),
                new SystemSetting(SettingKeys.ContactAddress,            request.ContactAddress   ?? string.Empty),
                new SystemSetting(SettingKeys.ContactFacebook,           request.ContactFacebook  ?? string.Empty),
                new SystemSetting(SettingKeys.ContactInstagram,          request.ContactInstagram ?? string.Empty),
                new SystemSetting(SettingKeys.ContactTwitter,            request.ContactTwitter   ?? string.Empty),
                new SystemSetting(SettingKeys.EmailConfirmationEnabled,  request.EmailConfirmationEnabled.ToString()),
                new SystemSetting(SettingKeys.ShowNotificationCount,     request.ShowNotificationCount.ToString()),
            };

            await _repository.UpsertManyAsync(updated);
            await _cacheService.RemoveAsync(CacheKey);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static string Get(Dictionary<string, string> map, string key, string defaultValue)
            => map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : defaultValue;

        private static bool GetBool(Dictionary<string, string> map, string key, bool defaultValue)
            => map.TryGetValue(key, out var v) && bool.TryParse(v, out var result) ? result : defaultValue;
    }
}
