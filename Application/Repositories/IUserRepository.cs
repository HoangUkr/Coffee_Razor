using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(string username);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllActiveAsync();
        Task<IEnumerable<User>> GetAllAsync();
        Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(string? searchTerm, int pageNumber, int pageSize, bool includeInactive = false);
        Task<bool> DeleteAsync(Guid id);
    }
}
