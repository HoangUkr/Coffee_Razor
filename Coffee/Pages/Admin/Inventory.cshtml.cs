using Application.DTOs.Item;
using Application.DTOs.ItemImages;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Pages.Admin
{
    public class InventoryModel : PageModel
    {
        private readonly IItemService _itemService;
        private readonly IItemImageService _itemImageService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<InventoryModel> _logger;

        public InventoryModel(
            IItemService itemService,
            IItemImageService itemImageService,
            ICategoryService categoryService,
            ILogger<InventoryModel> logger)
        {
            _itemService = itemService;
            _itemImageService = itemImageService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public PaginatedResult<ItemResponse> Items { get; set; } = new();
        public SelectList Categories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        [BindProperty]
        public int EditItemId { get; set; }

        [BindProperty]
        public EditItemInput EditInput { get; set; } = new();

        [BindProperty]
        public IFormFile? EditImageFile { get; set; }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var updateRequest = new UpdateItemRequest
                {
                    Name = EditInput.Name,
                    Price = EditInput.Price,
                    CategoryId = EditInput.CategoryId,
                    Description = EditInput.Description,
                    IsActive = EditInput.IsActive
                };

                var result = await _itemService.UpdateAsync(EditItemId, updateRequest);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToPage();
                }

                // Handle image upload if provided
                if (EditImageFile != null)
                {
                    await HandleImageUploadAsync(EditItemId, EditImageFile);
                }

                TempData["SuccessMessage"] = $"Item '{result.Name}' has been updated successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {ItemId}", EditItemId);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToPage();
                }

                await _itemService.DeleteAsync(id);
                TempData["SuccessMessage"] = $"Item '{item.Name}' has been deleted successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item: {ItemId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the item.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            try
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null)
                {
                    return new JsonResult(new { success = false, message = "Item not found" });
                }

                bool newStatus;
                if (item.IsActive)
                {
                    await _itemService.DeactivateAsync(id);
                    newStatus = false;
                }
                else
                {
                    await _itemService.ActivateAsync(id);
                    newStatus = true;
                }

                return new JsonResult(new
                {
                    success = true,
                    isActive = newStatus,
                    message = $"Item has been {(newStatus ? "activated" : "deactivated")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling item status: {ItemId}", id);
                return new JsonResult(new { success = false, message = "An error occurred" });
            }
        }

        private async Task HandleImageUploadAsync(int itemId, IFormFile imageFile)
        {
            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file extension: {Extension}", extension);
                    return;
                }

                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning("File size too large: {Size} bytes", imageFile.Length);
                    return;
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/items/{uniqueFileName}";
                var createImageRequest = new CreateItemImageRequest
                {
                    Url = imageUrl,
                    ItemId = itemId,
                    IsDefault = true
                };

                await _itemImageService.CreateAsync(createImageRequest);
                _logger.LogInformation("Item image uploaded successfully for ItemId: {ItemId}", itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for ItemId: {ItemId}", itemId);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var parameters = new SearchParameters
                {
                    SearchTerm = SearchTerm,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                Items = await _itemService.SearchAsync(parameters, includeInactive: true);

                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new SelectList(categories, "Id", "Name");

                if (TempData["SuccessMessage"] != null)
                {
                    SuccessMessage = TempData["SuccessMessage"]?.ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ErrorMessage = TempData["ErrorMessage"]?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory data");
                Items = new PaginatedResult<ItemResponse>();
                Categories = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        public class EditItemInput
        {
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}
