using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly CoffeeDbContext _context;

        public HolidayRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<Holiday>> GetAllActiveAsync()
            => await _context.Holidays
                .AsNoTracking()
                .Where(h => h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();

        public async Task<Holiday?> GetByIdAsync(int id)
            => await _context.Holidays.FindAsync(id);

        public async Task AddAsync(Holiday holiday)
        {
            await _context.Holidays.AddAsync(holiday);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Holiday holiday)
        {
            _context.Holidays.Update(holiday);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday is not null)
            {
                holiday.Deactivate();
                await _context.SaveChangesAsync();
            }
        }
    }
}
