using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrderItems
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        public int OrderId { get; private set; }

        [ForeignKey("OrderId")]
        public Order Order { get; private set; }

        [Required]
        public int ItemId { get; private set; }

        [ForeignKey("ItemId")]
        public Item Item { get; private set; }
        public decimal UnitPrice { get; private set; }
        public int Quantity { get; private set; }

        public OrderItems(int itemId, int quantity, decimal unitPrice)
        {
            if (itemId <= 0) throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
            if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

            ItemId = itemId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        private OrderItems() { }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity < 0) throw new ArgumentException("Quantity cannot be negative");
            Quantity = newQuantity;
        }
    }
}
