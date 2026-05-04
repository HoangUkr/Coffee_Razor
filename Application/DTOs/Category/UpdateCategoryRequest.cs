using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Category
{
    public record UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
        public string Name { get; init; } = string.Empty;

        public int Version { get; init; }
    }
}
