using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Item
{
    public record CreateItemRequest
    {
        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(200, ErrorMessage = "Item name cannot exceed 200 characters")]
        [MinLength(2, ErrorMessage = "Item name must be at least 2 characters")]
        public string Name { get; init; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        public decimal Price { get; init; }

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [MinLength(5, ErrorMessage = "Description must be at least 5 characters")]
        public string Description { get; init; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        public int CategoryId { get; init; }
    }
}
