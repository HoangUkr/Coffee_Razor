using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;

namespace Application.Repositories
{
    public interface IItemRepository
    {
        Task<Item?> GetByIdAsync(int id);
        Task<IEnumerable<Item>> GetAllAsync();
        Task<IEnumerable<Item>> GetAllActiveAsync();
        Task<IEnumerable<Item>> GetItemsByCategoryAsync(int categoryId);
        Task<(IEnumerable<Item> Items, int TotalCount)> SearchAsync(string? searchTerm, int pageNumber, int pageSize, bool includeInactive = false);
        Task<Item> CreateAsync(Item item);
        Task UpdateAsync(Item item, int originalVersion);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name);
    }
}
