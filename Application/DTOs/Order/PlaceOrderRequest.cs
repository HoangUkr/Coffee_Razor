using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Order
{
    public record PlaceOrderRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; init; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        public string LastName { get; init; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(200)]
        public string? Email { get; init; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(20)]
        public string PhoneNumber { get; init; } = string.Empty;

        public OrderFulfillmentScope FulfillmentScope { get; init; } = OrderFulfillmentScope.OutHouse;

        public OutHouseFulfillmentType? OutHouseFulfillmentType { get; init; } = Domain.Enums.OutHouseFulfillmentType.Pickup;

        [MaxLength(500)]
        public string? DeliveryAddress { get; init; }

        [MaxLength(1000)]
        public string? Notes { get; init; }

        [Required(ErrorMessage = "Order must contain at least one item")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemRequest> Items { get; init; } = new();
    }

    public record OrderItemRequest
    {
        [Required(ErrorMessage = "Item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Item ID must be greater than 0")]
        public int ItemId { get; init; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; init; }
    }
}
