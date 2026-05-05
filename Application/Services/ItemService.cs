using System.Diagnostics;
using Application.DTOs.Item;
using Application.DTOs.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ItemService : IItemService
    {
        internal const string ActiveItemsCacheKey = "items:active:all";
        private const string ItemDetailCacheKeyPrefix = "items:detail:";
        private static readonly TimeSpan ActiveItemsCacheDuration = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ItemDetailCacheDuration = TimeSpan.FromMinutes(5);
        private readonly IItemRepository _itemRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<ItemService> _logger;
        private readonly IStorageService _storageService;

        public ItemService(
            IItemRepository itemRepository,
            ICategoryRepository categoryRepository,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<ItemService> logger,
            IStorageService storageService)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public async Task<ItemResponse?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(id));

            var cacheKey = GetItemDetailCacheKey(id);
            var cachedItem = await _cacheService.GetAsync<ItemResponse>(cacheKey);
            if (cachedItem != null)
            {
                _logger.LogInformation("ITEM DETAIL SOURCE | ItemId: {ItemId} | Source: CACHE", id);
                return cachedItem;
            }

            var sw = Stopwatch.StartNew();
            var item = await _itemRepository.GetByIdAsync(id);
            sw.Stop();
            _logger.LogInformation("ITEM DETAIL SOURCE | ItemId: {ItemId} | Source: DB | Elapsed: {ElapsedMs}ms", id, sw.ElapsedMilliseconds);
            if (item == null)
            {
                return null;
            }

            var itemResponse = _mapper.Map<ItemResponse>(item, opts => opts.Items["StorageService"] = _storageService);
            await _cacheService.SetAsync(cacheKey, itemResponse, ItemDetailCacheDuration);
            return itemResponse;
        }

        public async Task<IEnumerable<ItemResponse>> GetAllAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ItemResponse>>(items, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<IEnumerable<ItemResponse>> GetAllActiveAsync()
        {
            var cachedItems = await _cacheService.GetAsync<List<ItemResponse>>(ActiveItemsCacheKey);
            if (cachedItems != null)
            {
                _logger.LogInformation("ACTIVE ITEMS SOURCE | Source: CACHE");
                return cachedItems;
            }

            var sw = Stopwatch.StartNew();
            var items = await _itemRepository.GetAllActiveAsync();
            sw.Stop();
            _logger.LogInformation("ACTIVE ITEMS SOURCE | Source: DB | Elapsed: {ElapsedMs}ms", sw.ElapsedMilliseconds);
            var itemResponses = _mapper.Map<List<ItemResponse>>(items, opts => opts.Items["StorageService"] = _storageService);
            await _cacheService.SetAsync(ActiveItemsCacheKey, itemResponses, ActiveItemsCacheDuration);
            return itemResponses;
        }

        public async Task<ItemResponse> CreateAsync(CreateItemRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if item with same name already exists
            if (await _itemRepository.ExistsAsync(request.Name))
            {
                throw new InvalidOperationException($"Item with name '{request.Name}' already exists");
            }

            // Validate category exists
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
            }

            // Create new item
            var item = new Item(request.Name, request.Price, request.CategoryId, request.Description);

            // Save to database
            var createdItem = await _itemRepository.CreateAsync(item);
            await InvalidateItemCachesAsync(createdItem.Id);

            // Reload with category to get full details
            var itemWithCategory = await _itemRepository.GetByIdAsync(createdItem.Id);

            // Map to response DTO
            return _mapper.Map<ItemResponse>(itemWithCategory!, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<ItemResponse?> UpdateAsync(int id, UpdateItemRequest request)
        {
            if (id <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Get existing item
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new InvalidOperationException($"Item with ID {id} not found");
            }

            // Check if another item with the same name exists
            var existingItem = await _itemRepository.ExistsAsync(request.Name);
            if (existingItem && !item.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Another item with name '{request.Name}' already exists");
            }

            // Validate category exists
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");
            }

            // Update item details
            item.UpdateDetails(request.Name, request.Price, request.CategoryId, request.Description ?? item.Description);

            // Update active status
            if (request.IsActive)
                item.Activate();
            else
                item.Deactivate();

            item.IncrementVersion();

            try
            {
                await _itemRepository.UpdateAsync(item, request.Version);
            }
            catch (DbUpdateConcurrencyException)
            {
                await InvalidateItemCachesAsync(id);
                throw new ConcurrencyConflictException("This item was updated by another admin. Your changes were not saved. Please reload and try again.");
            }

            // Invalidate cache first, then reload to ensure fresh data is cached
            await InvalidateItemCachesAsync(id);
            var updatedItem = await _itemRepository.GetByIdAsync(id);

            // Map to response DTO
            return _mapper.Map<ItemResponse>(updatedItem!, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(id));

            // Hard delete - actually remove from database
            var deleted = await _itemRepository.DeleteAsync(id);
            if (deleted)
            {
                await InvalidateItemCachesAsync(id);
            }

            return deleted;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(id));

            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
            {
                return false;
            }

            item.Activate();
            item.IncrementVersion();
            await _itemRepository.UpdateAsync(item, item.Version - 1);
            await InvalidateItemCachesAsync(id);

            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Item ID must be greater than 0", nameof(id));

            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
            {
                return false;
            }

            item.Deactivate();
            item.IncrementVersion();
            await _itemRepository.UpdateAsync(item, item.Version - 1);
            await InvalidateItemCachesAsync(id);

            return true;
        }

        public async Task<IEnumerable<ItemResponse>> GetItemsByCategoryAsync(int categoryId)
        {
            var items = await _itemRepository.GetItemsByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ItemResponse>>(items, opts => opts.Items["StorageService"] = _storageService);
        }

        public async Task<PaginatedResult<ItemResponse>> SearchAsync(SearchParameters parameters, bool includeInactive = false)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var (items, totalCount) = await _itemRepository.SearchAsync(
                parameters.SearchTerm,
                parameters.PageNumber,
                parameters.PageSize,
                includeInactive);

            var itemResponses = _mapper.Map<IEnumerable<ItemResponse>>(items, opts => opts.Items["StorageService"] = _storageService);

            return new PaginatedResult<ItemResponse>(
                itemResponses,
                totalCount,
                parameters.PageNumber,
                parameters.PageSize);
        }

        internal async Task InvalidateItemCachesAsync(int itemId)
        {
            await _cacheService.RemoveManyAsync(new[]
            {
                ActiveItemsCacheKey,
                GetItemDetailCacheKey(itemId)
            });
        }

        internal static string GetItemDetailCacheKey(int itemId) => $"{ItemDetailCacheKeyPrefix}{itemId}";
    }
}
