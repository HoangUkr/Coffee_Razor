using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CoffeeDbContext _context;
        public UserRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<bool> ExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }
        public async Task<User> CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        public async Task UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<User>> GetAllActiveAsync()
        {
            return await _context.Users.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Username).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> SearchAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            bool includeInactive = false)
        {
            // Build query with deferred execution
            var query = _context.Users.AsNoTracking();

            // Apply search filter if search term provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTermLower) ||
                    u.Role.ToLower().Contains(searchTermLower));
            }

            // Filter by active status
            if (!includeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var users = await query
                .OrderBy(u => u.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
