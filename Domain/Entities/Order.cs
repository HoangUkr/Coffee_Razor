using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Domain.Enums;

namespace Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        [MaxLength(5)]
        [Column(TypeName = "char(5)")]
        public string OrderCode { get; private set; }

        [Required]
        public Guid CustomerId { get; private set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; private set; }

        public OrderStatus Status { get; private set; }
        public OrderFulfillmentScope FulfillmentScope { get; private set; }
        public OutHouseFulfillmentType? OutHouseFulfillmentType { get; private set; }

        [MaxLength(500)]
        public string? DeliveryAddress { get; private set; }

        [MaxLength(1000)]
        public string? Notes { get; private set; }

        public decimal TotalPrice { get; private set; }
        public int TotalItemsAmount { get; private set; }
        public int Version { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }

        public bool IsCompleted { get; private set; }
        public DateTimeOffset? CompletedDate { get; private set; }

        public virtual ICollection<OrderItems> OrderItems { get; private set; } = new HashSet<OrderItems>();

        public Order(
            string orderCode,
            Guid customerId,
            OrderFulfillmentScope fulfillmentScope,
            OutHouseFulfillmentType? outHouseFulfillmentType = null,
            string? deliveryAddress = null,
            string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(orderCode) || orderCode.Length != 5)
                throw new ArgumentException("Order code must be exactly 5 characters", nameof(orderCode));
            if (customerId == Guid.Empty)
                throw new ArgumentException("Customer ID is required", nameof(customerId));
            if (fulfillmentScope == OrderFulfillmentScope.InHouse && outHouseFulfillmentType.HasValue)
                throw new ArgumentException("Out-house fulfillment type must be empty for in-house orders", nameof(outHouseFulfillmentType));
            if (fulfillmentScope == OrderFulfillmentScope.OutHouse && !outHouseFulfillmentType.HasValue)
                throw new ArgumentException("Out-house fulfillment type is required for out-house orders", nameof(outHouseFulfillmentType));
            if (fulfillmentScope == OrderFulfillmentScope.OutHouse &&
                outHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery &&
                string.IsNullOrWhiteSpace(deliveryAddress))
                throw new ArgumentException("Delivery address is required for delivery orders", nameof(deliveryAddress));

            OrderCode = orderCode.ToUpper();
            CustomerId = customerId;
            Status = OrderStatus.Pending;
            FulfillmentScope = fulfillmentScope;
            OutHouseFulfillmentType = fulfillmentScope == OrderFulfillmentScope.OutHouse ? outHouseFulfillmentType : null;
            DeliveryAddress = fulfillmentScope == OrderFulfillmentScope.OutHouse && outHouseFulfillmentType == Domain.Enums.OutHouseFulfillmentType.Delivery
                ? deliveryAddress?.Trim()
                : null;
            Notes = notes?.Trim();
            TotalPrice = 0;
            TotalItemsAmount = 0;
            CreatedDate = DateTimeOffset.UtcNow;
            IsCompleted = false;
            OrderItems = new HashSet<OrderItems>();
        }

        private Order() { }

        private void UpdateTotals()
        {
            TotalPrice = OrderItems.Sum(i => i.UnitPrice * i.Quantity);
            TotalItemsAmount = OrderItems.Sum(i => i.Quantity);
        }

        public void AddItem(OrderItems item)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Cannot modify a completed order");

            var existingItem = OrderItems.FirstOrDefault(i => i.ItemId == item.ItemId);
            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + item.Quantity);
            }
            else
            {
                OrderItems.Add(item);
            }
            UpdateTotals();
        }

        public void UpdateItem(int itemId, int newQuantity)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Cannot modify a completed order");

            var item = OrderItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                item.UpdateQuantity(newQuantity);
                UpdateTotals();
            }
        }

        public void RemoveItem(int itemId)
        {
            if (IsCompleted)
                throw new InvalidOperationException("Cannot modify a completed order");

            var item = OrderItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                OrderItems.Remove(item);
                UpdateTotals();
            }
        }

        public void CompleteOrder()
        {
            if (IsCompleted)
                throw new InvalidOperationException("Order is already completed");

            Status = OrderStatus.Completed;
            IsCompleted = true;
            CompletedDate = DateTimeOffset.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            if (IsCompleted && newStatus != OrderStatus.Completed)
                throw new InvalidOperationException("Cannot change status of a completed order");

            Status = newStatus;

            if (newStatus == OrderStatus.Completed)
            {
                IsCompleted = true;
                CompletedDate = DateTimeOffset.UtcNow;
            }
        }

        public void ClearDeliveryAddress()
        {
            DeliveryAddress = null;
        }

        public void IncrementVersion()
        {
            Version++;
        }
    }
}
