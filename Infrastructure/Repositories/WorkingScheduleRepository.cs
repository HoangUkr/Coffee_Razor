using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WorkingScheduleRepository : IWorkingScheduleRepository
    {
        private readonly CoffeeDbContext _context;

        public WorkingScheduleRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<WorkingSchedule>> GetAllAsync()
            => await _context.WorkingSchedules.AsNoTracking().ToListAsync();

        public async Task UpsertManyAsync(IEnumerable<WorkingSchedule> schedules)
        {
            foreach (var schedule in schedules)
            {
                var existing = await _context.WorkingSchedules.FindAsync(schedule.Day);
                if (existing == null)
                    _context.WorkingSchedules.Add(schedule);
                else
                    existing.Update(schedule.OpenTime, schedule.CloseTime, schedule.IsClosed);
            }

            await _context.SaveChangesAsync();
        }
    }
}
