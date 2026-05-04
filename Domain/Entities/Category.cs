using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; private set; }
        [Required]
        public string Name { get; private set; }
        public int DisplayOrder { get; private set; }
        public int Version { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public virtual ICollection<Item> Items { get; private set; } = new HashSet<Item>();

        public Category(string name, int displayOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty", nameof(name));

            Name = name.Trim();
            DisplayOrder = displayOrder;
            CreatedDate = DateTimeOffset.UtcNow;
            Items = new HashSet<Item>();
        }

        private Category() { }

        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Category name cannot be empty", nameof(newName));

            Name = newName.Trim();
        }

        public void UpdateDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }

        public void IncrementVersion()
        {
            Version++;
        }
    }
}
