using Domain.Entities;

namespace Application.Repositories
{
    public interface ISystemSettingRepository
    {
        Task<IEnumerable<SystemSetting>> GetAllAsync();
        Task UpsertManyAsync(IEnumerable<SystemSetting> settings);
    }
}
