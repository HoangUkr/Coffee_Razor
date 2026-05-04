using Domain.Entities;

namespace Application.Repositories
{
    public interface IItemImageRepository
    {
        Task<ItemImages?> GetByIdAsync(int id);
        Task<IEnumerable<ItemImages>> GetByItemIdAsync(int itemId);
        Task<IEnumerable<ItemImages>> GetActiveByItemIdAsync(int itemId);
        Task<ItemImages?> GetDefaultByItemIdAsync(int itemId);
        Task<ItemImages> CreateAsync(ItemImages itemImage);
        Task<ItemImages> UpdateAsync(ItemImages itemImage);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountByItemIdAsync(int itemId);
    }
}
