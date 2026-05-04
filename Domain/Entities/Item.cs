using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Item
    {
        [Key]
        public int Id { get; private set; }
        [Required]
        public string Name { get; private set; }
        [Required]
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; }
        public int Version { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }
        [Required]
        public int CategoryId { get; private set; }
        [Required]
        public string Description { get; private set; } = string.Empty;

        [ForeignKey("CategoryId")]
        public Category Category { get; private set; }
        public virtual ICollection<ItemImages> ItemImages { get; private set; } = new HashSet<ItemImages>();

        public Item(string name, decimal price, int categoryId, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));
            if (categoryId <= 0)
                throw new ArgumentException("CategoryId must be greater than 0", nameof(categoryId));

            Name = name.Trim();
            Price = price;
            CategoryId = categoryId;
            IsActive = true;
            CreatedDate = DateTimeOffset.UtcNow;
            ItemImages = new HashSet<ItemImages>();
            Description = description;
        }

        private Item() { }

        public void UpdateDetails(string name, decimal price, int categoryId, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));
            if (categoryId <= 0)
                throw new ArgumentException("CategoryId must be greater than 0", nameof(categoryId));

            Name = name.Trim();
            Price = price;
            CategoryId = categoryId;
            Description = description.Trim();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void IncrementVersion()
        {
            Version++;
        }
    }
}
