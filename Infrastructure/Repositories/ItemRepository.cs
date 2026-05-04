using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly CoffeeDbContext _context;
        public ItemRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items
                        .AsSplitQuery() // Use split query for multiple collections
                        .AsNoTracking()
                        .Include(i => i.Category)
                        .Include(i => i.ItemImages)
                        .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Items
                        .AsSplitQuery() // Use split query for multiple collections
                        .AsNoTracking()
                        .Include(i => i.Category)
                        .Include(i => i.ItemImages)
                        .OrderBy(i => i.Name)
                        .ToListAsync();
        }

        public async Task<IEnumerable<Item>> GetAllActiveAsync()
        {
            return await _context.Items
                        .AsSplitQuery() // Use split query for multiple collections
                        .AsNoTracking()
                        .Include(i => i.Category)
                        .Include(i => i.ItemImages)
                        .Where(i => i.IsActive)
                        .OrderBy(i => i.Name)
                        .ToListAsync();
        }
        public async Task<IEnumerable<Item>> GetItemsByCategoryAsync(int categoryId)
        {
            return await _context.Items
                        .AsSplitQuery() // Use split query for multiple collections
                        .AsNoTracking()
                        .Include(i => i.Category)
                        .Include(i => i.ItemImages)
                        .Where(i => i.CategoryId == categoryId && i.IsActive)
                        .OrderBy(i => i.Name)
                        .ToListAsync();
        }
        public async Task<Item> CreateAsync(Item item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task UpdateAsync(Item item, int originalVersion)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _context.Items.Attach(item);
            var entry = _context.Entry(item);
            entry.Property(i => i.Version).OriginalValue = originalVersion;
            entry.State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<bool> ExistsAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return await _context.Items.AnyAsync(i => i.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0) return false;

            var item = await _context.Items
                .Include(i => i.ItemImages)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return false;

            // Remove related ItemImages first (cascade delete may not be configured)
            if (item.ItemImages != null && item.ItemImages.Any())
            {
                _context.ItemImages.RemoveRange(item.ItemImages);
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Item> Items, int TotalCount)> SearchAsync(
            string? searchTerm, 
            int pageNumber, 
            int pageSize, 
            bool includeInactive = false)
        {
            // Build query with deferred execution for optimization
            IQueryable<Item> query = _context.Items
                .AsSplitQuery() // Use split query for multiple collections
                .AsNoTracking()
                .Include(i => i.Category)
                .Include(i => i.ItemImages);

            // Apply search filter if search term provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower();
                query = query.Where(i => 
                    i.Name.ToLower().Contains(searchTermLower) ||
                    i.Description.ToLower().Contains(searchTermLower) ||
                    i.Category.Name.ToLower().Contains(searchTermLower));
            }

            // Filter by active status
            if (!includeInactive)
            {
                query = query.Where(i => i.IsActive);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var items = await query
                .OrderBy(i => i.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
