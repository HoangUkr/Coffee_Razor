using Application.DTOs.Item;
using Application.DTOs.Common;

namespace Application.Interfaces
{
    public interface IItemService
    {
        Task<ItemResponse?> GetByIdAsync(int id);
        Task<IEnumerable<ItemResponse>> GetAllAsync();
        Task<IEnumerable<ItemResponse>> GetAllActiveAsync();
        Task<IEnumerable<ItemResponse>> GetItemsByCategoryAsync(int categoryId);
        Task<PaginatedResult<ItemResponse>> SearchAsync(SearchParameters parameters, bool includeInactive = false);
        Task<ItemResponse> CreateAsync(CreateItemRequest request);
        Task<ItemResponse?> UpdateAsync(int id, UpdateItemRequest request);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
