using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly CoffeeDbContext _context;

        public SystemSettingRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<SystemSetting>> GetAllAsync()
            => await _context.SystemSettings.AsNoTracking().ToListAsync();

        public async Task UpsertManyAsync(IEnumerable<SystemSetting> settings)
        {
            foreach (var setting in settings)
            {
                var existing = await _context.SystemSettings.FindAsync(setting.Key);
                if (existing == null)
                {
                    _context.SystemSettings.Add(setting);
                }
                else
                {
                    existing.SetValue(setting.Value);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
