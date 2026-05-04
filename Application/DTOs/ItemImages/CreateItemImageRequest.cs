using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ItemImages
{
    public record CreateItemImageRequest
    {
        [Required(ErrorMessage = "Image URL/filename is required")]
        [StringLength(500, ErrorMessage = "URL/filename cannot exceed 500 characters")]
        public string Url { get; init; } = string.Empty;

        [Required(ErrorMessage = "Item ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid item")]
        public int ItemId { get; init; }

        public bool IsDefault { get; init; } = false;
    }
}
