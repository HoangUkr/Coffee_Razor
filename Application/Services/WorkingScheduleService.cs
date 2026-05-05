using Application.DTOs.Settings;
using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class WorkingScheduleService : IWorkingScheduleService
    {
        private const string CacheKey = "system:schedule";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

        // Ordered Mon → Sun for consistent display
        private static readonly DayOfWeek[] OrderedDays =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        ];

        private readonly IWorkingScheduleRepository _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<WorkingScheduleService> _logger;

        public WorkingScheduleService(
            IWorkingScheduleRepository repository,
            ICacheService cacheService,
            ILogger<WorkingScheduleService> logger)
        {
            _repository   = repository   ?? throw new ArgumentNullException(nameof(repository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger       = logger       ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<WorkingScheduleEntry>> GetScheduleAsync()
        {
            var cached = await _cacheService.GetAsync<List<WorkingScheduleEntry>>(CacheKey);
            if (cached != null)
            {
                _logger.LogInformation("SCHEDULE SOURCE | Source: CACHE");
                return cached;
            }

            var rows = await _repository.GetAllAsync();
            var map  = rows.ToDictionary(r => r.Day);

            var schedule = OrderedDays.Select(day =>
            {
                if (map.TryGetValue(day, out var row))
                    return ToEntry(row);

                // Fallback default if seed is missing
                return new WorkingScheduleEntry
                {
                    Day       = day,
                    DayName   = day.ToString(),
                    OpenTime  = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(22, 0),
                    IsClosed  = false
                };
            }).ToList();

            await _cacheService.SetAsync(CacheKey, schedule, CacheDuration);
            _logger.LogInformation("SCHEDULE SOURCE | Source: DB");
            return schedule;
        }

        public async Task UpdateScheduleAsync(IEnumerable<UpdateScheduleDayRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));

            var schedules = requests.Select(r => new WorkingSchedule(
                r.Day,
                TimeOnly.Parse(r.OpenTime),
                TimeOnly.Parse(r.CloseTime),
                r.IsClosed
            ));

            await _repository.UpsertManyAsync(schedules);
            await _cacheService.RemoveAsync(CacheKey);
        }

        private static WorkingScheduleEntry ToEntry(WorkingSchedule s) => new()
        {
            Day       = s.Day,
            DayName   = s.Day.ToString(),
            OpenTime  = s.OpenTime,
            CloseTime = s.CloseTime,
            IsClosed  = s.IsClosed
        };
    }
}
