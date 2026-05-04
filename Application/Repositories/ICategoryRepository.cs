using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetByNameAsync(string name);
        Task<Category> CreateAsync(Category category);
        Task<Category> UpdateAsync(Category category, int originalVersion);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<(IEnumerable<Category> Categories, int TotalCount)> SearchAsync(string? searchTerm, int pageNumber, int pageSize);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
    }
}
