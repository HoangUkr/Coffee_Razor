using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ItemImages
{
    public record UpdateItemImageRequest
    {
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Please provide a valid URL")]
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string Url { get; init; } = string.Empty;

        public bool? IsDefault { get; init; }

        public bool? IsActive { get; init; }
    }
}
