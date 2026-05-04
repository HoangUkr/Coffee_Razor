using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CoffeeDbContext _context;
        public OrderRepository(CoffeeDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<Order?> GetByIdWithDetailsAsync(int orderId)
        {
            return await _context.Orders.AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                        .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            // This method is now for getting orders by CustomerId
            return await _context.Orders.AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderItems)
                        .Where(o => o.CustomerId == userId)
                        .OrderByDescending(o => o.CreatedDate)
                        .ToListAsync();
        }

        public async Task<Order?> GetByOrderCodeAsync(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode)) return null;
            return await _context.Orders
                        .AsSplitQuery() // Use split query for multiple collections
                        .AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                        .ThenInclude(i => i.Category)
                        .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                        .ThenInclude(i => i.ItemImages)
                        .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
        {
            return await _context.Orders.AsNoTracking()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderItems)
                        .OrderByDescending(o => o.CreatedDate)
                        .Take(count)
                        .ToListAsync();
        }
        public async Task UpdateAsync(Order order, int originalVersion)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            _context.Orders.Attach(order);
            var entry = _context.Entry(order);
            entry.Property(o => o.Version).OriginalValue = originalVersion;
            entry.State = EntityState.Modified;

            if (order.Customer != null)
            {
                _context.Entry(order.Customer).State = EntityState.Modified;
            }

            foreach (var orderItem in order.OrderItems)
            {
                var orderItemEntry = _context.Entry(orderItem);
                orderItemEntry.State = orderItem.Id == 0 ? EntityState.Added : EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersWithFilterAsync(
            string? customerCode = null,
            string? orderCode = null,
            DateTime? createdDate = null,
            OrderStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedDate",
            bool sortDescending = true)
        {
            var query = _context.Orders
                .AsSplitQuery() // Use split query for multiple collections
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.Category)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .ThenInclude(i => i.ItemImages)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(customerCode))
            {
                query = query.Where(o => o.Customer.CustomerCode.Contains(customerCode));
            }

            if (!string.IsNullOrWhiteSpace(orderCode))
            {
                query = query.Where(o => o.OrderCode.Contains(orderCode));
            }

            if (createdDate.HasValue)
            {
                var startOfDay = createdDate.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(o => o.CreatedDate >= startOfDay && o.CreatedDate < endOfDay);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "ordercode" => sortDescending ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode),
                "customercode" => sortDescending ? query.OrderByDescending(o => o.Customer.CustomerCode) : query.OrderBy(o => o.Customer.CustomerCode),
                "status" => sortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                "completeddate" => sortDescending ? query.OrderByDescending(o => o.CompletedDate) : query.OrderBy(o => o.CompletedDate),
                _ => sortDescending ? query.OrderByDescending(o => o.CreatedDate) : query.OrderBy(o => o.CreatedDate)
            };

            // Apply pagination
            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}
