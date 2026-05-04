using Application.DTOs.Category;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class CreateCategoryModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CreateCategoryModel> _logger;

        public CreateCategoryModel(
            ICategoryService categoryService,
            INotificationService notificationService,
            ILogger<CreateCategoryModel> logger)
        {
            _categoryService = categoryService;
            _notificationService = notificationService;
            _logger = logger;
        }

        [BindProperty]
        public CreateCategoryRequest Input { get; set; } = new CreateCategoryRequest();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if category already exists
                var existingCategory = await _categoryService.GetCategoryByNameAsync(Input.Name);
                if (existingCategory != null)
                {
                    ErrorMessage = $"Category '{Input.Name}' already exists.";
                    return Page();
                }

                var result = await _categoryService.CreateCategoryAsync(Input);

                _logger.LogInformation("Category created successfully: {CategoryName} (ID: {CategoryId})", result.Name, result.Id);

                await _notificationService.CreateForAdminsAsync("Category", $"Category '{result.Name}' created", "/Admin/Categories");

                TempData["SuccessMessage"] = $"Category '{result.Name}' has been created successfully!";
                return RedirectToPage("/Admin/Categories");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {CategoryName}", Input.Name);
                ErrorMessage = "An error occurred while creating the category. Please try again.";
                return Page();
            }
        }
    }
}
