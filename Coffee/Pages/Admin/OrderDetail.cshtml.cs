using Application.DTOs.Order;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class OrderDetailModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderDetailModel> _logger;

        public OrderDetailModel(
            INotificationService notificationService,
            IOrderService orderService,
            ILogger<OrderDetailModel> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _logger = logger;
        }

        public OrderDetailResponse? Order { get; set; }

        [BindProperty(SupportsGet = true)]
        public string OrderCode { get; set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(OrderCode))
            {
                ErrorMessage = "Order code is required.";
                return RedirectToPage("/Admin/Orders");
            }

            try
            {
                Order = await _orderService.GetOrderByCodeAsync(OrderCode);

                if (Order == null)
                {
                    ErrorMessage = $"Order '{OrderCode}' not found.";
                    return RedirectToPage("/Admin/Orders");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for {OrderCode}", OrderCode);
                ErrorMessage = "Failed to load order details. Please try again.";
                return RedirectToPage("/Admin/Orders");
            }
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string orderCode, OrderStatus newStatus, int version)
        {
            try
            {
                var order = await _orderService.GetOrderByCodeAsync(orderCode);
                if (order == null)
                {
                    ErrorMessage = $"Order '{orderCode}' not found.";
                    return RedirectToPage("/Admin/Orders");
                }

                // If marking as completed, clear customer personal data
                if (newStatus == OrderStatus.Completed)
                {
                    await _orderService.CompleteOrderAndClearDataAsync(order.Id, version);
                    await _notificationService.CreateForAdminsAsync("Orders", $"Order {orderCode} marked as completed", $"/Admin/OrderDetail?orderCode={orderCode}");
                    SuccessMessage = "Order marked as completed and customer data cleared!";
                }
                else
                {
                    await _orderService.UpdateOrderStatusAsync(order.Id, newStatus, version);
                    await _notificationService.CreateForAdminsAsync("Orders", $"Order {orderCode} status changed to {newStatus}", $"/Admin/OrderDetail?orderCode={orderCode}");
                    SuccessMessage = "Order status updated successfully!";
                }
            }
            catch (ConcurrencyConflictException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating order status for {OrderCode}", orderCode);
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderCode}", orderCode);
                ErrorMessage = "Failed to update order status. Please try again.";
            }

            return RedirectToPage(new { orderCode });
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
    }
}
