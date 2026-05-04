using Application.DTOs.Category;
using Application.DTOs.Item;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages
{
    public class MenuModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;

        public MenuModel(ICategoryService categoryService, IItemService itemService)
        {
            _categoryService = categoryService;
            _itemService = itemService;
        }

        public List<CategoryResponse> Categories { get; set; } = new();
        public Dictionary<int, List<ItemResponse>> ItemsByCategory { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get all categories
            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories = categories.ToList();

            // Get items for each category
            foreach (var category in Categories)
            {
                var items = await _itemService.GetItemsByCategoryAsync(category.Id);
                ItemsByCategory[category.Id] = items.ToList();
            }
        }
    }
}
