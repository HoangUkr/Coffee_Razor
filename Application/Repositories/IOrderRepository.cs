using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Domain.Enums;

namespace Application.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdWithDetailsAsync(int orderId);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<Order?> GetByOrderCodeAsync(string orderCode);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order, int originalVersion);
        Task<IEnumerable<Order>> GetRecentOrdersAsync(int count=10);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersWithFilterAsync(
            string? customerCode = null,
            string? orderCode = null,
            DateTime? createdDate = null,
            OrderStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedDate",
            bool sortDescending = true);
    }
}
