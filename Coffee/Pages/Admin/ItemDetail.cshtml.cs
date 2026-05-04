using Application.DTOs.Item;
using Application.DTOs.ItemImages;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Pages.Admin
{
    public class ItemDetailModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IItemService _itemService;
        private readonly IItemImageService _itemImageService;
        private readonly ICategoryService _categoryService;
        private readonly IStorageService _storageService;
        private readonly ILogger<ItemDetailModel> _logger;

        public ItemDetailModel(
            INotificationService notificationService,
            IItemService itemService,
            IItemImageService itemImageService,
            ICategoryService categoryService,
            IStorageService storageService,
            ILogger<ItemDetailModel> logger)
        {
            _notificationService = notificationService;
            _itemService = itemService;
            _itemImageService = itemImageService;
            _categoryService = categoryService;
            _storageService = storageService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public ItemResponse Item { get; set; } = null!;
        public IEnumerable<ItemImageResponse> ItemImages { get; set; } = new List<ItemImageResponse>();
        public SelectList Categories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public UpdateItemInput Input { get; set; } = new();

        [BindProperty]
        public IFormFile? NewImageFile { get; set; }

        [BindProperty]
        public int? SelectedDefaultImageId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
            {
                return RedirectToPage("/Admin/Inventory");
            }

            await LoadDataAsync();

            if (Item == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToPage("/Admin/Inventory");
            }

            // Populate form input with current item data
            Input = new UpdateItemInput
            {
                Name = Item.Name,
                Description = Item.Description,
                Price = Item.Price,
                CategoryId = Item.CategoryId,
                IsActive = Item.IsActive,
                Version = Item.Version
            };

            // Set the current default image
            var defaultImage = ItemImages.FirstOrDefault(i => i.IsDefault);
            SelectedDefaultImageId = defaultImage?.Id;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateDetailsAsync()
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
                    Name = Input.Name,
                    Description = Input.Description,
                    Price = Input.Price,
                    CategoryId = Input.CategoryId,
                    IsActive = Input.IsActive,
                    Version = Input.Version
                };

                var result = await _itemService.UpdateAsync(Id, updateRequest);

                if (result == null)
                {
                    ErrorMessage = "Item not found.";
                    await LoadDataAsync();
                    return Page();
                }

                // Update default image if changed
                if (SelectedDefaultImageId.HasValue)
                {
                    var currentDefault = await _itemImageService.GetDefaultByItemIdAsync(Id);
                    if (currentDefault == null || currentDefault.Id != SelectedDefaultImageId.Value)
                    {
                        await _itemImageService.SetAsDefaultAsync(SelectedDefaultImageId.Value);
                    }
                }

                await _notificationService.CreateForAdminsAsync("Inventory", $"Item '{result.Name}' updated", $"/Admin/ItemDetail?id={Id}");

                SuccessMessage = $"Item '{result.Name}' has been updated successfully!";
                return RedirectToPage(new { Id });
            }
            catch (ConcurrencyConflictException ex)
            {
                ErrorMessage = ex.Message;
                await LoadDataAsync();
                if (Item != null)
                {
                    Input.Version = Item.Version;
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item: {ItemId}", Id);
                ErrorMessage = ex.Message;
                await LoadDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            if (NewImageFile == null)
            {
                ErrorMessage = "Please select an image file.";
                return RedirectToPage(new { Id });
            }

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(NewImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ErrorMessage = "Invalid file type. Only JPG, PNG, GIF, and WebP images are allowed.";
                    return RedirectToPage(new { Id });
                }

                if (NewImageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    ErrorMessage = "File size must be less than 5MB.";
                    return RedirectToPage(new { Id });
                }

                // Upload to Azure Blob Storage - returns filename only
                var fileName = await _storageService.UploadImageAsync(NewImageFile, NewImageFile.FileName);
                _logger.LogInformation("Image uploaded to cloud for ItemId {ItemId}: {FileName}", Id, fileName);

                // Create ItemImage record with filename (not full URL)
                var createImageRequest = new CreateItemImageRequest
                {
                    Url = fileName, // Store filename for dynamic SAS URL generation
                    ItemId = Id,
                    IsDefault = false // Don't make it default automatically
                };

                await _itemImageService.CreateAsync(createImageRequest);
                SuccessMessage = "Image uploaded successfully to cloud storage!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for ItemId: {ItemId}", Id);
                ErrorMessage = "An error occurred while uploading the image.";
            }

            return RedirectToPage(new { Id });
        }

        public async Task<IActionResult> OnPostToggleImageAsync(int imageId, bool isActive)
        {
            try
            {
                bool result;
                if (isActive)
                {
                    result = await _itemImageService.DeactivateAsync(imageId);
                    SuccessMessage = result ? "Image has been deactivated." : "Image not found.";
                }
                else
                {
                    result = await _itemImageService.ActivateAsync(imageId);
                    SuccessMessage = result ? "Image has been activated." : "Image not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling image: {ImageId}", imageId);
                ErrorMessage = "An error occurred while toggling the image.";
            }

            return RedirectToPage(new { Id });
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
        {
            try
            {
                // Get the image first to get its URL/filename
                var image = await _itemImageService.GetByIdAsync(imageId);

                if (image == null)
                {
                    ErrorMessage = "Image not found.";
                    return RedirectToPage(new { Id });
                }

                // Delete from Azure Storage (supports both filename and full URL)
                if (!string.IsNullOrEmpty(image.Url))
                {
                    var deleted = await _storageService.DeleteImageAsync(image.Url);
                    if (deleted)
                    {
                        _logger.LogInformation("Image deleted from cloud storage: {Url}", image.Url);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete image from cloud storage: {Url}", image.Url);
                    }
                }

                // Delete the database record
                var result = await _itemImageService.DeleteAsync(imageId);
                if (result)
                {
                    SuccessMessage = "Image has been deleted from cloud and database.";
                }
                else
                {
                    ErrorMessage = "Failed to delete image record.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageId}", imageId);
                ErrorMessage = "An error occurred while deleting the image.";
            }

            return RedirectToPage(new { Id });
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Item = await _itemService.GetByIdAsync(Id);
                ItemImages = (await _itemImageService.GetByItemIdAsync(Id))
                    .Select(image => image with
                    {
                        Url = image.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                              image.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                            ? image.Url
                            : _storageService.GetImageUrl(image.Url)
                    })
                    .ToList();

                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new SelectList(categories, "Id", "Name");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item data for ItemId: {ItemId}", Id);
            }
        }
    }

    public class UpdateItemInput
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
    }
}
