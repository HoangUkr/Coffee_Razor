using Application.DTOs.Settings;
using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class HolidayService : IHolidayService
    {
        private const string CacheKey = "system:holidays";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

        private readonly IHolidayRepository _repository;
        private readonly ICacheService _cache;
        private readonly ILogger<HolidayService> _logger;

        public HolidayService(
            IHolidayRepository repository,
            ICacheService cache,
            ILogger<HolidayService> logger)
        {
            _repository = repository;
            _cache      = cache;
            _logger     = logger;
        }

        public async Task<IReadOnlyList<HolidayResponse>> GetAllActiveAsync()
        {
            var holidays = await GetCachedAsync();
            return holidays.OrderBy(h => h.Date).ToList();
        }

        public async Task<HolidayResponse?> GetByIdAsync(int id)
        {
            var holidays = await GetCachedAsync();
            return holidays.FirstOrDefault(h => h.Id == id);
        }

        public async Task<HolidayResponse?> GetHolidayForDateAsync(DateOnly date)
        {
            var holidays = await GetCachedAsync();
            return holidays.FirstOrDefault(h => MatchesDate(h, date));
        }

        public async Task<int> CreateAsync(CreateHolidayRequest request)
        {
            var holiday = new Holiday(request.Date, request.Name, request.IsRecurring);
            await _repository.AddAsync(holiday);
            await InvalidateCacheAsync();
            return holiday.Id;
        }

        public async Task UpdateAsync(int id, UpdateHolidayRequest request)
        {
            var holiday = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Holiday {id} not found.");

            holiday.Update(request.Date, request.Name, request.IsRecurring);
            await _repository.UpdateAsync(holiday);
            await InvalidateCacheAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            await InvalidateCacheAsync();
        }

        // ── private ──────────────────────────────────────────────────────────

        private async Task<IReadOnlyList<HolidayResponse>> GetCachedAsync()
        {
            var cached = await _cache.GetAsync<List<HolidayResponse>>(CacheKey);
            if (cached != null)
                return cached;

            var entities = await _repository.GetAllActiveAsync();
            var dtos = entities.Select(MapToResponse).ToList();
            await _cache.SetAsync(CacheKey, dtos, CacheDuration);
            return dtos;
        }

        private async Task InvalidateCacheAsync()
        {
            try { await _cache.RemoveAsync(CacheKey); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to invalidate holiday cache."); }
        }

        private static bool MatchesDate(HolidayResponse h, DateOnly target) =>
            h.IsActive &&
            (h.Date == target ||
             (h.IsRecurring && h.Date.Month == target.Month && h.Date.Day == target.Day));

        private static HolidayResponse MapToResponse(Holiday h) => new()
        {
            Id          = h.Id,
            Date        = h.Date,
            Name        = h.Name,
            IsRecurring = h.IsRecurring,
            IsActive    = h.IsActive,
        };
    }
}
