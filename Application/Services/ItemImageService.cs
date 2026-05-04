using Application.DTOs.ItemImages;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class ItemImageService : IItemImageService
    {
        private readonly IItemImageRepository _itemImageRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public ItemImageService(
            IItemImageRepository itemImageRepository,
            IItemRepository itemRepository,
            ICacheService cacheService,
            IMapper mapper)
        {
            _itemImageRepository = itemImageRepository ?? throw new ArgumentNullException(nameof(itemImageRepository));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ItemImageResponse?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            return itemImage != null ? _mapper.Map<ItemImageResponse>(itemImage) : null;
        }

        public async Task<IEnumerable<ItemImageResponse>> GetByItemIdAsync(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            var itemImages = await _itemImageRepository.GetByItemIdAsync(itemId);
            return _mapper.Map<IEnumerable<ItemImageResponse>>(itemImages);
        }

        public async Task<IEnumerable<ItemImageResponse>> GetActiveByItemIdAsync(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            var itemImages = await _itemImageRepository.GetActiveByItemIdAsync(itemId);
            return _mapper.Map<IEnumerable<ItemImageResponse>>(itemImages);
        }

        public async Task<ItemImageResponse?> GetDefaultByItemIdAsync(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(itemId));

            var itemImage = await _itemImageRepository.GetDefaultByItemIdAsync(itemId);
            return itemImage != null ? _mapper.Map<ItemImageResponse>(itemImage) : null;
        }

        public async Task<ItemImageResponse> CreateAsync(CreateItemImageRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate that the item exists
            var item = await _itemRepository.GetByIdAsync(request.ItemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {request.ItemId} not found");
            }

            // If this is set as default, unset any existing default for this item
            if (request.IsDefault)
            {
                await UnsetExistingDefaultForItem(request.ItemId);
            }
            // If no images exist for this item yet, make this the default
            else
            {
                var imageCount = await _itemImageRepository.CountByItemIdAsync(request.ItemId);
                if (imageCount == 0)
                {
                    // First image should be default
                    var firstImage = new ItemImages(request.Url, request.ItemId, true);
                    var createdFirstImage = await _itemImageRepository.CreateAsync(firstImage);
                    await InvalidateItemCachesAsync(request.ItemId);
                    var reloadedFirst = await _itemImageRepository.GetByIdAsync(createdFirstImage.Id);
                    return _mapper.Map<ItemImageResponse>(reloadedFirst!);
                }
            }

            // Create new item image
            var itemImage = new ItemImages(request.Url, request.ItemId, request.IsDefault);

            // Save to database
            var createdItemImage = await _itemImageRepository.CreateAsync(itemImage);
            await InvalidateItemCachesAsync(request.ItemId);

            // Reload with item details
            var reloaded = await _itemImageRepository.GetByIdAsync(createdItemImage.Id);

            // Map to response DTO
            return _mapper.Map<ItemImageResponse>(reloaded!);
        }

        public async Task<ItemImageResponse?> UpdateAsync(int id, UpdateItemImageRequest request)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing item image
            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            if (itemImage == null)
            {
                throw new InvalidOperationException($"Item image with ID {id} not found");
            }

            // Update URL
            itemImage.UpdateUrl(request.Url);

            // Update default status if provided
            if (request.IsDefault.HasValue)
            {
                if (request.IsDefault.Value)
                {
                    // Unset existing default for this item
                    await UnsetExistingDefaultForItem(itemImage.ItemId);
                    itemImage.SetAsDefault();
                }
                else
                {
                    itemImage.UnsetAsDefault();
                }
            }

            // Update active status if provided
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    itemImage.Activate();
                else
                    itemImage.Deactivate();
            }

            // Save changes
            await _itemImageRepository.UpdateAsync(itemImage);
            await InvalidateItemCachesAsync(itemImage.ItemId);

            // Reload with item details
            var updated = await _itemImageRepository.GetByIdAsync(id);

            // Map to response DTO
            return _mapper.Map<ItemImageResponse>(updated!);
        }

        public async Task<bool> SetAsDefaultAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            if (itemImage == null)
            {
                return false;
            }

            // Unset existing default for this item
            await UnsetExistingDefaultForItem(itemImage.ItemId);

            // Set this as default
            itemImage.SetAsDefault();
            await _itemImageRepository.UpdateAsync(itemImage);
            await InvalidateItemCachesAsync(itemImage.ItemId);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            if (itemImage == null)
            {
                return false;
            }

            // If this was the default image, we need to set another image as default
            if (itemImage.IsDefault)
            {
                var otherImages = await _itemImageRepository.GetActiveByItemIdAsync(itemImage.ItemId);
                var newDefault = otherImages.FirstOrDefault(i => i.Id != id);
                if (newDefault != null)
                {
                    newDefault.SetAsDefault();
                    await _itemImageRepository.UpdateAsync(newDefault);
                }
            }

            var deleted = await _itemImageRepository.DeleteAsync(id);
            if (deleted)
            {
                await InvalidateItemCachesAsync(itemImage.ItemId);
            }

            return deleted;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            if (itemImage == null)
            {
                return false;
            }

            itemImage.Activate();
            await _itemImageRepository.UpdateAsync(itemImage);
            await InvalidateItemCachesAsync(itemImage.ItemId);

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Image ID must be greater than 0", nameof(id));

            var itemImage = await _itemImageRepository.GetByIdAsync(id);
            if (itemImage == null)
            {
                return false;
            }

            // Don't allow deactivating the default image unless it's the only image
            if (itemImage.IsDefault)
            {
                var activeImages = await _itemImageRepository.GetActiveByItemIdAsync(itemImage.ItemId);
                if (activeImages.Count() > 1)
                {
                    throw new InvalidOperationException("Cannot deactivate the default image. Please set another image as default first.");
                }
            }

            itemImage.Deactivate();
            await _itemImageRepository.UpdateAsync(itemImage);
            await InvalidateItemCachesAsync(itemImage.ItemId);

            return true;
        }

        private async Task UnsetExistingDefaultForItem(int itemId)
        {
            var existingDefault = await _itemImageRepository.GetDefaultByItemIdAsync(itemId);
            if (existingDefault != null)
            {
                existingDefault.UnsetAsDefault();
                await _itemImageRepository.UpdateAsync(existingDefault);
            }
        }

        private Task InvalidateItemCachesAsync(int itemId)
        {
            return _cacheService.RemoveManyAsync(new[]
            {
                ItemService.ActiveItemsCacheKey,
                ItemService.GetItemDetailCacheKey(itemId)
            });
        }
    }
}
