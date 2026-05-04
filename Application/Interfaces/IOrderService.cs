using Application.DTOs.Common;
using Application.DTOs.Order;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        /// <summary>
        /// Places a new order
        /// </summary>
        Task<OrderDetailResponse> PlaceOrderAsync(PlaceOrderRequest request, Guid userId);

        /// <summary>
        /// Gets order details by ID
        /// </summary>
        Task<OrderDetailResponse?> GetOrderDetailsAsync(int orderId);

        /// <summary>
        /// Gets all orders for a user
        /// </summary>
        Task<IEnumerable<OrderSummaryResponse>> GetUserOrdersAsync(Guid userId);

        /// <summary>
        /// Gets order by order code
        /// </summary>
        Task<OrderDetailResponse?> GetOrderByCodeAsync(string orderCode);

        Task<OrderDetailResponse> OnAddItem(int orderId, int itemId, int version);
        Task<OrderDetailResponse> OnRemoveItem(int orderId, int itemId, int version);
        Task<OrderDetailResponse> OnUpdateItemQuantity(int orderId, int itemId, int newQuantity, int version);

        /// <summary>
        /// Updates order status
        /// </summary>
        Task<OrderDetailResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int version);

        /// <summary>
        /// Completes order and clears customer personal data
        /// </summary>
        Task CompleteOrderAndClearDataAsync(int orderId, int version);

        /// <summary>
        /// Gets paginated orders with filters for admin panel
        /// </summary>
        Task<PaginatedResult<OrderListResponse>> GetOrdersWithFilterAsync(
            string? customerCode = null,
            string? orderCode = null,
            DateTime? createdDate = null,
            OrderStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string sortBy = "CreatedDate",
            bool sortDescending = true);

        /// <summary>
        /// Creates an order manually (admin)
        /// </summary>
        Task<OrderDetailResponse> CreateOrderManuallyAsync(PlaceOrderRequest request);
    }
}
