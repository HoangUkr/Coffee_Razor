using Application.DTOs.Common;
using Application.DTOs.Order;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class OrdersModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersModel> _logger;

        public OrdersModel(
            INotificationService notificationService,
            IOrderService orderService,
            ILogger<OrdersModel> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _logger = logger;
        }

        public PaginatedResult<OrderListResponse> PaginatedOrders { get; set; } = new();
        public List<OrderListResponse> Orders => PaginatedOrders.Items.ToList();

        [BindProperty(SupportsGet = true)]
        public string? CustomerCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? OrderCode { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? CreatedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public OrderStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "CreatedDate";

        [BindProperty(SupportsGet = true)]
        public bool SortDescending { get; set; } = true;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadOrdersAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, OrderStatus newStatus, int version)
        {
            try
            {
                // If marking as completed, clear customer personal data
                if (newStatus == OrderStatus.Completed)
                {
                    await _orderService.CompleteOrderAndClearDataAsync(orderId, version);
                    await _notificationService.CreateForAdminsAsync("Orders", $"Order #{orderId} marked as completed", "/Admin/Orders");
                    SuccessMessage = "Order marked as completed and customer data cleared!";
                }
                else
                {
                    await _orderService.UpdateOrderStatusAsync(orderId, newStatus, version);
                    await _notificationService.CreateForAdminsAsync("Orders", $"Order #{orderId} status changed to {newStatus}", "/Admin/Orders");
                    SuccessMessage = "Order status updated successfully!";
                }
            }
            catch (ConcurrencyConflictException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating order status for order {OrderId}", orderId);
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                ErrorMessage = "Failed to update order status. Please try again.";
            }

            return RedirectToPage(new
            {
                CustomerCode,
                OrderCode,
                CreatedDate = CreatedDate?.ToString("yyyy-MM-dd"),
                StatusFilter,
                PageNumber,
                PageSize,
                SortBy,
                SortDescending
            });
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                PaginatedOrders = await _orderService.GetOrdersWithFilterAsync(
                    CustomerCode,
                    OrderCode,
                    CreatedDate,
                    StatusFilter,
                    PageNumber,
                    PageSize,
                    SortBy,
                    SortDescending
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                ErrorMessage = "Failed to load orders. Please try again.";
                PaginatedOrders = new PaginatedResult<OrderListResponse>();
            }
        }

        public string GetNextStatus(OrderStatus currentStatus, OrderFulfillmentScope fulfillmentScope, OutHouseFulfillmentType? outHouseFulfillmentType)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => OrderStatus.InProgress.ToString(),
                OrderStatus.InProgress => fulfillmentScope == OrderFulfillmentScope.InHouse
                    ? OrderStatus.Completed.ToString()
                    : outHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Pickup
                        ? OrderStatus.ReadyToPickup.ToString()
                        : OrderStatus.OnTheWay.ToString(),
                OrderStatus.ReadyToPickup => OrderStatus.Completed.ToString(),
                OrderStatus.OnTheWay => OrderStatus.Completed.ToString(),
                OrderStatus.Completed => string.Empty,
                _ => string.Empty
            };
        }

        public string GetStatusBadgeClass(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "bg-warning",
                OrderStatus.InProgress => "bg-info",
                OrderStatus.ReadyToPickup => "bg-primary",
                OrderStatus.OnTheWay => "bg-primary",
                OrderStatus.Completed => "bg-success",
                _ => "bg-secondary"
            };
        }

        public string GetStatusDisplayName(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.InProgress => "In Progress",
                OrderStatus.ReadyToPickup => "Ready to Pickup",
                OrderStatus.OnTheWay => "On the Way",
                OrderStatus.Completed => "Completed",
                _ => status.ToString()
            };
        }
    }
}
