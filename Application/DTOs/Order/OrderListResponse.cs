using Domain.Enums;

namespace Application.DTOs.Order
{
    public class OrderListResponse
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int Version { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? CompletedDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItemsAmount { get; set; }
        public OrderFulfillmentScope FulfillmentScope { get; set; }
        public OutHouseFulfillmentType? OutHouseFulfillmentType { get; set; }
    }
}
