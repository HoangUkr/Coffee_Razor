using Application.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ItemImageRepository : IItemImageRepository
    {
        private readonly CoffeeDbContext _context;

        public ItemImageRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ItemImages?> GetByIdAsync(int id)
        {
            return await _context.ItemImages
                .AsNoTracking()
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<ItemImages>> GetByItemIdAsync(int itemId)
        {
            return await _context.ItemImages
                .AsNoTracking()
                .Include(i => i.Item)
                .Where(i => i.ItemId == itemId)
                .OrderByDescending(i => i.IsDefault)
                .ThenBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemImages>> GetActiveByItemIdAsync(int itemId)
        {
            return await _context.ItemImages
                .AsNoTracking()
                .Include(i => i.Item)
                .Where(i => i.ItemId == itemId && i.IsActive)
                .OrderByDescending(i => i.IsDefault)
                .ThenBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<ItemImages?> GetDefaultByItemIdAsync(int itemId)
        {
            return await _context.ItemImages
                .AsNoTracking()
                .Include(i => i.Item)
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.IsDefault && i.IsActive);
        }

        public async Task<ItemImages> CreateAsync(ItemImages itemImage)
        {
            if (itemImage == null)
                throw new ArgumentNullException(nameof(itemImage));

            await _context.ItemImages.AddAsync(itemImage);
            await _context.SaveChangesAsync();
            return itemImage;
        }

        public async Task<ItemImages> UpdateAsync(ItemImages itemImage)
        {
            if (itemImage == null)
                throw new ArgumentNullException(nameof(itemImage));

            // Detach any existing tracked ItemImages entity
            var existingImageEntry = _context.ChangeTracker.Entries<ItemImages>()
                .FirstOrDefault(e => e.Entity.Id == itemImage.Id);

            if (existingImageEntry != null)
            {
                existingImageEntry.State = EntityState.Detached;
            }

            // Detach any existing tracked Item entity (related to ItemImages)
            if (itemImage.Item != null)
            {
                var existingItemEntry = _context.ChangeTracker.Entries<Item>()
                    .FirstOrDefault(e => e.Entity.Id == itemImage.ItemId);

                if (existingItemEntry != null)
                {
                    existingItemEntry.State = EntityState.Detached;
                }
            }

            _context.ItemImages.Update(itemImage);
            await _context.SaveChangesAsync();
            return itemImage;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var itemImage = await _context.ItemImages.FindAsync(id);
            if (itemImage == null)
                return false;

            _context.ItemImages.Remove(itemImage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ItemImages.AnyAsync(i => i.Id == id);
        }

        public async Task<int> CountByItemIdAsync(int itemId)
        {
            return await _context.ItemImages
                .Where(i => i.ItemId == itemId && i.IsActive)
                .CountAsync();
        }
    }
}
