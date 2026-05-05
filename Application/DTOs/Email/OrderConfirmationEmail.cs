namespace Application.DTOs.Email
{
    public record OrderConfirmationEmail
    {
        public string ToEmail { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string OrderCode { get; init; } = string.Empty;
        public decimal TotalPrice { get; init; }
        public string FulfillmentDescription { get; init; } = string.Empty;
        public string? DeliveryAddress { get; init; }
        public string? Notes { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        public List<OrderItemEmailLine> Items { get; init; } = new();
    }

    public record OrderItemEmailLine(string ItemName, int Quantity, decimal UnitPrice, decimal LineTotal);
}
