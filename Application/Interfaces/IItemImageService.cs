using Application.DTOs.ItemImages;

namespace Application.Interfaces
{
    public interface IItemImageService
    {
        Task<ItemImageResponse?> GetByIdAsync(int id);
        Task<IEnumerable<ItemImageResponse>> GetByItemIdAsync(int itemId);
        Task<IEnumerable<ItemImageResponse>> GetActiveByItemIdAsync(int itemId);
        Task<ItemImageResponse?> GetDefaultByItemIdAsync(int itemId);
        Task<ItemImageResponse> CreateAsync(CreateItemImageRequest request);
        Task<ItemImageResponse?> UpdateAsync(int id, UpdateItemImageRequest request);
        Task<bool> SetAsDefaultAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
