using Application.DTOs.Item;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace WebUI.Pages
{
    public class CartModel : PageModel
    {
        private readonly IItemService _itemService;
        private const string CartSessionKey = "ShoppingCart";

        public CartModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public List<ItemResponse> RelatedProducts { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public async Task OnGetAsync()
        {
            // Load cart from session
            CartItems = GetCartFromSession();

            // Load item details for each cart item
            foreach (var cartItem in CartItems)
            {
                var item = await _itemService.GetByIdAsync(cartItem.ItemId);
                if (item != null)
                {
                    cartItem.Name = item.Name;
                    cartItem.Price = item.Price;
                    cartItem.ImageUrl = item.ImageUrl ?? "/images/menu-1.jpg";
                    cartItem.Description = item.Description;
                }
            }

            // Calculate totals
            CalculateTotals();

            // Load related/recommended products
            var allItems = await _itemService.GetAllActiveAsync();
            RelatedProducts = allItems.Take(4).ToList();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int itemId, int quantity = 1)
        {
            var item = await _itemService.GetByIdAsync(itemId);
            if (item == null || !item.IsActive)
            {
                return BadRequest(new { message = "Item not found or not available" });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(c => c.ItemId == itemId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ItemId = itemId,
                    Quantity = quantity,
                    Name = item.Name,
                    Price = item.Price,
                    ImageUrl = item.ImageUrl ?? "/images/menu-1.jpg"
                });
            }

            SaveCartToSession(cart);

            return new JsonResult(new { success = true, itemCount = cart.Sum(c => c.Quantity) });
        }

        public IActionResult OnPostUpdateQuantity(int itemId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ItemId == itemId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Remove(item);
                }

                SaveCartToSession(cart);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveItem(int itemId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(c => c.ItemId == itemId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToPage();
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

        private void SaveCartToSession(List<CartItemViewModel> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        private void CalculateTotals()
        {
            Subtotal = CartItems.Sum(item => item.Price * item.Quantity);
            DeliveryFee = 0; // Free delivery
            Discount = Subtotal > 20 ? 3 : 0; // $3 discount for orders over $20
            Total = Subtotal + DeliveryFee - Discount;
        }
    }

    public class CartItemViewModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal LineTotal => Price * Quantity;
    }
}
