using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages
{
    public class OrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private const string OrderConfirmationSessionKey = "JustPlacedOrderCode";

        public OrderConfirmationModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Application.DTOs.Order.OrderDetailResponse? Order { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return RedirectToPage("/Index");
            }

            // Security: Only allow viewing if this order was just placed in this session
            var sessionOrderCode = HttpContext.Session.GetString(OrderConfirmationSessionKey);

            if (sessionOrderCode != orderCode)
            {
                // Order code doesn't match session - unauthorized access attempt
                return RedirectToPage("/Index");
            }

            // Clear the session key so the order can only be viewed once
            HttpContext.Session.Remove(OrderConfirmationSessionKey);

            Order = await _orderService.GetOrderByCodeAsync(orderCode);

            if (Order == null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        /// <summary>
        /// Helper method to set the order code in session after successful checkout
        /// Call this from Checkout.cshtml.cs after order is placed
        /// </summary>
        public static void SetOrderCodeInSession(ISession session, string orderCode)
        {
            session.SetString(OrderConfirmationSessionKey, orderCode);
        }
    }
}
