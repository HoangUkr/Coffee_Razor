using Application.DTOs.Category;
using Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponse?> GetCategoryByIdAsync(int id);
        Task<CategoryResponse?> GetCategoryByNameAsync(string name);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<PaginatedResult<CategoryResponse>> SearchAsync(SearchParameters parameters);
        Task<bool> UpdateCategoryOrderAsync(Dictionary<int, int> categoryOrders);
    }
}
