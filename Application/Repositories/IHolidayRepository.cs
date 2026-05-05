using Domain.Entities;

namespace Application.Repositories
{
    public interface IHolidayRepository
    {
        Task<IReadOnlyList<Holiday>> GetAllActiveAsync();
        Task<Holiday?> GetByIdAsync(int id);
        Task AddAsync(Holiday holiday);
        Task UpdateAsync(Holiday holiday);
        Task DeleteAsync(int id);
    }
}
