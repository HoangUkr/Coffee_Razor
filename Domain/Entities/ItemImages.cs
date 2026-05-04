using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ItemImages
    {
        [Key]
        public int Id { get; private set; }

        [Required]
        [MaxLength(500)]
        public string Url { get; private set; }

        [Required]
        public int ItemId { get; private set; }

        [ForeignKey("ItemId")]
        public Item Item { get; private set; }

        public bool IsDefault { get; private set; } = false;
        public bool IsActive { get; private set; } = true;
        public DateTimeOffset CreatedDate { get; private set; }

        public ItemImages(string url, int itemId, bool isDefault = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be empty", nameof(url));
            if (itemId <= 0)
                throw new ArgumentException("ItemId must be greater than 0", nameof(itemId));

            Url = url.Trim();
            ItemId = itemId;
            IsDefault = isDefault;
            IsActive = true;
            CreatedDate = DateTimeOffset.UtcNow;
        }

        private ItemImages() { }

        public void UpdateUrl(string newUrl)
        {
            if (string.IsNullOrWhiteSpace(newUrl))
                throw new ArgumentException("URL cannot be empty", nameof(newUrl));
            Url = newUrl.Trim();
        }

        public void SetAsDefault()
        {
            IsDefault = true;
        }

        public void UnsetAsDefault()
        {
            IsDefault = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
