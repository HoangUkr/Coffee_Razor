using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.DTOs.Order;
using Domain.Enums;
using System.Text.Json;

namespace WebUI.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IItemService _itemService;
        private readonly ILogger<CheckoutModel> _logger;
        private const string CartSessionKey = "ShoppingCart";

        public CheckoutModel(INotificationService notificationService, IOrderService orderService, IItemService itemService, ILogger<CheckoutModel> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _itemService = itemService;
            _logger = logger;
        }

        [BindProperty]
        public CheckoutInputModel Input { get; set; } = new();

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CartItems = GetCartFromSession();

            if (!CartItems.Any())
            {
                ErrorMessage = "Your cart is empty.";
                return RedirectToPage("/Cart");
            }

            // Load item details
            foreach (var cartItem in CartItems)
            {
                var item = await _itemService.GetByIdAsync(cartItem.ItemId);
                if (item != null)
                {
                    cartItem.Name = item.Name;
                    cartItem.Price = item.Price;
                    cartItem.ImageUrl = item.ImageUrl ?? "/images/menu-1.jpg";
                    cartItem.Description = item.Description ?? "";
                }
            }

            CalculateTotals();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CartItems = GetCartFromSession();

            if (!CartItems.Any())
            {
                ErrorMessage = "Your cart is empty.";
                return RedirectToPage("/Cart");
            }

            // Load item details for display
            foreach (var cartItem in CartItems)
            {
                var item = await _itemService.GetByIdAsync(cartItem.ItemId);
                if (item != null)
                {
                    cartItem.Name = item.Name;
                    cartItem.Price = item.Price;
                    cartItem.ImageUrl = item.ImageUrl ?? "/images/menu-1.jpg";
                    cartItem.Description = item.Description ?? "";
                }
            }

            CalculateTotals();

            // Validate delivery address for out-house delivery orders
            if (Input.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
                Input.OutHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery &&
                string.IsNullOrWhiteSpace(Input.Address))
            {
                ModelState.AddModelError("Input.Address", "Delivery address is required for delivery orders.");
            }

            if (Input.FulfillmentScope == OrderFulfillmentScope.OutHouse && !Input.OutHouseFulfillmentType.HasValue)
            {
                ModelState.AddModelError("Input.OutHouseFulfillmentType", "Please choose pickup or delivery for out-house orders.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Create order request
                var orderRequest = new PlaceOrderRequest
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.Phone,
                    FulfillmentScope = Input.FulfillmentScope,
                    OutHouseFulfillmentType = Input.FulfillmentScope == OrderFulfillmentScope.OutHouse ? Input.OutHouseFulfillmentType : null,
                    DeliveryAddress = Input.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
                                      Input.OutHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery
                        ? Input.Address
                        : null,
                    Notes = Input.Notes,
                    Items = CartItems.Select(c => new OrderItemRequest
                    {
                        ItemId = c.ItemId,
                        Quantity = c.Quantity
                    }).ToList()
                };

                // Use empty Guid for guest orders
                var order = await _orderService.PlaceOrderAsync(orderRequest, Guid.Empty);

                // Clear cart after successful order
                HttpContext.Session.Remove(CartSessionKey);

                // Security: Store order code in session to allow viewing confirmation page only once
                OrderConfirmationModel.SetOrderCodeInSession(HttpContext.Session, order.OrderCode);

                await _notificationService.CreateForAdminsAsync("Orders", $"Customer placed order {order.OrderCode}", $"/Admin/OrderDetail?orderCode={order.OrderCode}");

                _logger.LogInformation($"Order {order.OrderCode} placed successfully");

                SuccessMessage = $"Order {order.OrderCode} placed successfully! We will contact you soon.";
                return RedirectToPage("/OrderConfirmation", new { orderCode = order.OrderCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                ErrorMessage = ex.Message;
                return Page();
            }
        }

        private List<CartItemViewModel> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItemViewModel>();
            }

            return JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
        }

        private void CalculateTotals()
        {
            Subtotal = CartItems.Sum(item => item.Price * item.Quantity);
            Total = Subtotal;
        }
    }

    public class CheckoutInputModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200)]
        [Display(Name = "Email (Optional)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Fulfillment Scope")]
        public OrderFulfillmentScope FulfillmentScope { get; set; } = OrderFulfillmentScope.OutHouse;

        [Display(Name = "Out-House Fulfillment Type")]
        public OutHouseFulfillmentType? OutHouseFulfillmentType { get; set; } = Domain.Enums.OutHouseFulfillmentType.Pickup;

        [StringLength(500)]
        [Display(Name = "Delivery Address")]
        public string? Address { get; set; }

        [StringLength(1000)]
        [Display(Name = "Special Instructions")]
        public string? Notes { get; set; }
    }
}
