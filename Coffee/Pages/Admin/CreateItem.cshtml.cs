using Application.DTOs.Item;
using Application.DTOs.ItemImages;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Pages.Admin
{
    public class CreateItemModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IItemService _itemService;
        private readonly IItemImageService _itemImageService;
        private readonly ICategoryService _categoryService;
        private readonly IStorageService _storageService;
        private readonly ILogger<CreateItemModel> _logger;

        public CreateItemModel(
            INotificationService notificationService,
            IItemService itemService,
            IItemImageService itemImageService,
            ICategoryService categoryService,
            IStorageService storageService,
            ILogger<CreateItemModel> logger)
        {
            _notificationService = notificationService;
            _itemService = itemService;
            _itemImageService = itemImageService;
            _categoryService = categoryService;
            _storageService = storageService;
            _logger = logger;
        }

        [BindProperty]
        public CreateItemRequest Input { get; set; } = new CreateItemRequest();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public SelectList Categories { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                // Create the item first
                var result = await _itemService.CreateAsync(Input);
                _logger.LogInformation("Item created successfully: {ItemName} (ID: {ItemId})", result.Name, result.Id);

                // Handle image upload if provided
                if (ImageFile != null)
                {
                    await HandleImageUploadAsync(result.Id, ImageFile);
                }

                await _notificationService.CreateForAdminsAsync("Inventory", $"Item '{result.Name}' created", $"/Admin/ItemDetail?id={result.Id}");

                TempData["SuccessMessage"] = $"Item '{result.Name}' has been created successfully!";
                return RedirectToPage("/Admin/Inventory");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating item: {ItemName}", Input.Name);
                ErrorMessage = ex.Message;
                await LoadCategoriesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item: {ItemName}", Input.Name);
                ErrorMessage = "An error occurred while creating the item. Please try again.";
                await LoadCategoriesAsync();
                return Page();
            }
        }

        private async Task HandleImageUploadAsync(int itemId, IFormFile imageFile)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file extension: {Extension}", extension);
                    return;
                }

                if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                {
                    _logger.LogWarning("File size too large: {Size} bytes", imageFile.Length);
                    return;
                }

                // Upload to Azure Blob Storage - returns filename only
                var fileName = await _storageService.UploadImageAsync(imageFile, imageFile.FileName);
                _logger.LogInformation("Image uploaded to cloud: {FileName}", fileName);

                // Create ItemImage record with filename (not full URL)
                var createImageRequest = new CreateItemImageRequest
                {
                    Url = fileName, // Store filename for dynamic SAS URL generation
                    ItemId = itemId,
                    IsDefault = true // Set as default since it's the first/only image
                };

                await _itemImageService.CreateAsync(createImageRequest);
                _logger.LogInformation("Item image record created for ItemId: {ItemId}", itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for ItemId: {ItemId}", itemId);
                // Don't throw - item was created successfully, just log the error
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories = new SelectList(categories, "Id", "Name");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                Categories = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}
