using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Application.DTOs.Order
{
    public record OrderItemResponse
    {
        public int Id { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public string? ItemImage { get; init; }
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public record OrderDetailResponse
    {
        public int Id { get; init; }
        public string OrderCode { get; init; } = string.Empty;
        public int Version { get; init; }
        public decimal TotalPrice { get; init; }
        public int TotalItemsAmount { get; init; }
        public string CustomerCode { get; init; } = string.Empty;
        public string CustomerFirstName { get; init; } = string.Empty;
        public string CustomerLastName { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string? CustomerEmail { get; init; }
        public string? CustomerPhone { get; init; }
        public OrderStatus Status { get; init; }
        public OrderFulfillmentScope FulfillmentScope { get; init; }
        public OutHouseFulfillmentType? OutHouseFulfillmentType { get; init; }
        public string? DeliveryAddress { get; init; }
        public string? Notes { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public DateTimeOffset? CompletedDate { get; init; }
        public List<OrderItemResponse> Items { get; init; } = new();
    }
}
