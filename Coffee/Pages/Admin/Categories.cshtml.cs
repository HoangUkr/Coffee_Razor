using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class CategoriesModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CategoriesModel> _logger;

        public CategoriesModel(
            ICategoryService categoryService,
            INotificationService notificationService,
            ILogger<CategoriesModel> logger)
        {
            _categoryService = categoryService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public PaginatedResult<CategoryResponse> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        [BindProperty]
        public UpdateCategoryRequest? EditInput { get; set; }

        [BindProperty]
        public int EditCategoryId { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCategoriesAsync();

            // Check for success message from TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid || EditInput == null)
            {
                ErrorMessage = "Invalid category data.";
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                // Check if category with same name already exists (excluding current category)
                var existingCategory = await _categoryService.GetCategoryByNameAsync(EditInput.Name);
                if (existingCategory != null && existingCategory.Id != EditCategoryId)
                {
                    ErrorMessage = $"Category '{EditInput.Name}' already exists.";
                    await LoadCategoriesAsync();
                    return Page();
                }

                var result = await _categoryService.UpdateCategoryAsync(EditCategoryId, EditInput);

                if (result != null)
                {
                    _logger.LogInformation("Category updated successfully: {CategoryName} (ID: {CategoryId})", result.Name, result.Id);
                    await _notificationService.CreateForAdminsAsync("Category", $"Category '{result.Name}' updated", "/Admin/Categories");
                    TempData["SuccessMessage"] = $"Category '{result.Name}' has been updated successfully!";
                    return RedirectToPage();
                }
                else
                {
                    ErrorMessage = "Category not found.";
                }
            }
            catch (ConcurrencyConflictException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating category: {CategoryId}", EditCategoryId);
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", EditCategoryId);
                ErrorMessage = "An error occurred while updating the category. Please try again.";
            }

            await LoadCategoriesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToPage();
                }

                var result = await _categoryService.DeleteCategoryAsync(id);

                if (result)
                {
                    _logger.LogInformation("Category deleted successfully: {CategoryName} (ID: {CategoryId})", category.Name, id);
                    await _notificationService.CreateForAdminsAsync("Category", $"Category '{category.Name}' deleted", "/Admin/Categories");
                    TempData["SuccessMessage"] = $"Category '{category.Name}' has been deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete category. It may be in use by products.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category. It may be in use by products.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReorderAsync([FromBody] List<int> orderedIds)
        {
            try
            {
                if (orderedIds == null || !orderedIds.Any())
                {
                    return new JsonResult(new { success = false, message = "Invalid order data." });
                }

                // Create a dictionary mapping category ID to display order (0-based index)
                var categoryOrders = new Dictionary<int, int>();
                for (int i = 0; i < orderedIds.Count; i++)
                {
                    categoryOrders[orderedIds[i]] = i;
                }

                // Update the display order in the database
                var result = await _categoryService.UpdateCategoryOrderAsync(categoryOrders);

                if (result)
                {
                    _logger.LogInformation("Categories reordered successfully. New order: {OrderedIds}", string.Join(", ", orderedIds));
                    await _notificationService.CreateForAdminsAsync("Category", "Categories reordered", "/Admin/Categories");
                    return new JsonResult(new { success = true, message = "Categories reordered successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update category order." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering categories");
                return new JsonResult(new { success = false, message = "An error occurred while reordering categories." });
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var parameters = new SearchParameters
                {
                    SearchTerm = SearchTerm,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                Categories = await _categoryService.SearchAsync(parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                Categories = new PaginatedResult<CategoryResponse>();
                ErrorMessage = "An error occurred while loading categories.";
            }
        }
    }
}
