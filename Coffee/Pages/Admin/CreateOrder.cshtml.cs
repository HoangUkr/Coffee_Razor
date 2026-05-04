using Application.DTOs.Item;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class CreateOrderModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IItemService _itemService;
        private readonly ILogger<CreateOrderModel> _logger;

        public CreateOrderModel(
            INotificationService notificationService,
            IOrderService orderService,
            IItemService itemService,
            ILogger<CreateOrderModel> logger)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _itemService = itemService;
            _logger = logger;
        }

        [BindProperty]
        public OrderInputModel Input { get; set; } = new();

        public List<ItemResponse> AvailableItems { get; set; } = new();
        public List<ItemCategoryFilterModel> AvailableCategories { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAvailableItemsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableItemsAsync();
                return Page();
            }

            try
            {
                // Validate at least one item
                if (Input.Items == null || !Input.Items.Any(i => i.Quantity > 0))
                {
                    ModelState.AddModelError(string.Empty, "Please add at least one item to the order.");
                    await LoadAvailableItemsAsync();
                    return Page();
                }

                // Build PlaceOrderRequest
                if (Input.FulfillmentScope == OrderFulfillmentScope.OutHouse && !Input.OutHouseFulfillmentType.HasValue)
                {
                    ModelState.AddModelError("Input.OutHouseFulfillmentType", "Please choose pickup or delivery for out-house orders.");
                }

                if (Input.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
                    Input.OutHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery &&
                    string.IsNullOrWhiteSpace(Input.DeliveryAddress))
                {
                    ModelState.AddModelError("Input.DeliveryAddress", "Delivery address is required for delivery orders.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadAvailableItemsAsync();
                    return Page();
                }

                var request = new PlaceOrderRequest
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = Input.Email,
                    PhoneNumber = Input.PhoneNumber,
                    FulfillmentScope = Input.FulfillmentScope,
                    OutHouseFulfillmentType = Input.FulfillmentScope == OrderFulfillmentScope.OutHouse ? Input.OutHouseFulfillmentType : null,
                    DeliveryAddress = Input.FulfillmentScope == OrderFulfillmentScope.OutHouse &&
                                      Input.OutHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery
                        ? Input.DeliveryAddress
                        : null,
                    Notes = Input.Notes,
                    Items = Input.Items
                        .Where(i => i.Quantity > 0)
                        .Select(i => new OrderItemRequest { ItemId = i.ItemId, Quantity = i.Quantity })
                        .ToList()
                };

                var order = await _orderService.CreateOrderManuallyAsync(request);
                await _notificationService.CreateForAdminsAsync("Orders", $"Admin created order {order.OrderCode}", $"/Admin/OrderDetail?orderCode={order.OrderCode}");
                SuccessMessage = $"Order {order.OrderCode} created successfully!";
                return RedirectToPage("/Admin/OrderDetail", new { orderCode = order.OrderCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual order");
                ErrorMessage = $"Failed to create order: {ex.Message}";
                await LoadAvailableItemsAsync();
                return Page();
            }
        }

        private async Task LoadAvailableItemsAsync()
        {
            AvailableItems = (await _itemService.GetAllActiveAsync()).ToList();

            AvailableCategories = AvailableItems
                .GroupBy(i => new { i.CategoryId, i.CategoryName })
                .Select(g => new ItemCategoryFilterModel
                {
                    Id = g.Key.CategoryId,
                    Name = g.Key.CategoryName
                })
                .OrderBy(c => c.Name)
                .ToList();

            // Initialize Items if empty
            if (Input.Items == null || !Input.Items.Any())
            {
                Input.Items = AvailableItems.Select(i => new OrderItemInputModel
                {
                    ItemId = i.Id,
                    ItemName = i.Name,
                    UnitPrice = i.Price,
                    Quantity = 0
                }).ToList();
            }
            else
            {
                foreach (var inputItem in Input.Items)
                {
                    var item = AvailableItems.FirstOrDefault(i => i.Id == inputItem.ItemId);
                    if (item != null)
                    {
                        inputItem.ItemName = item.Name;
                        inputItem.UnitPrice = item.Price;
                    }
                }
            }
        }

        public string FormatCurrency(decimal amount) => $"${amount:F2}";

        public class OrderInputModel
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public OrderFulfillmentScope FulfillmentScope { get; set; } = OrderFulfillmentScope.OutHouse;
            public OutHouseFulfillmentType? OutHouseFulfillmentType { get; set; } = Domain.Enums.OutHouseFulfillmentType.Pickup;
            public string? DeliveryAddress { get; set; }
            public string? Notes { get; set; }
            public List<OrderItemInputModel> Items { get; set; } = new();
        }

        public class OrderItemInputModel
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }

        public class ItemCategoryFilterModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
