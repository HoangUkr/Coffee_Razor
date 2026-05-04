using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.Exceptions;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private const string CategoriesCacheKey = "categories:all";
        private static readonly TimeSpan CategoriesCacheDuration = TimeSpan.FromMinutes(30);
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ICacheService cacheService, IMapper mapper, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));

            var category = await _categoryRepository.GetByIdAsync(id);
            return category != null ? _mapper.Map<CategoryResponse>(category) : null;
        }

        public async Task<CategoryResponse?> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

            var category = await _categoryRepository.GetByNameAsync(name);
            return category != null ? _mapper.Map<CategoryResponse>(category) : null;
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if category with the same name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
            }

            // Get the max display order and add 1
            var allCategories = await _categoryRepository.GetAllAsync();
            var maxDisplayOrder = allCategories.Any() ? allCategories.Max(c => c.DisplayOrder) : 0;

            // Create new category entity
            var category = new Category(request.Name, maxDisplayOrder + 1);

            // Save to database
            var createdCategory = await _categoryRepository.CreateAsync(category);
            await InvalidateCategoryCachesAsync();

            // Map to response DTO
            return _mapper.Map<CategoryResponse>(createdCategory);
        }

        public async Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            if (id <= 0)
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("New name cannot be empty or whitespace.", nameof(request));

            // Get existing category
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new InvalidOperationException($"Category with ID {id} not found");
            }

            // Check if another category with the same name exists
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
            if (existingCategory != null && existingCategory.Id != id)
            {
                throw new InvalidOperationException($"Another category with name '{request.Name}' already exists");
            }

            // Update the category name
            category.UpdateName(request.Name);
            category.IncrementVersion();

            try
            {
                var updatedCategory = await _categoryRepository.UpdateAsync(category, request.Version);
                await InvalidateCategoryCachesAsync();

                // Map to response DTO
                return _mapper.Map<CategoryResponse>(updatedCategory);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyConflictException("This category was updated by another admin. Your changes were not saved. Please reload and try again.");
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));

            // Check if category exists
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Note: You might want to check if there are items associated with this category
            // and prevent deletion if there are, or cascade delete/update items

            var deleted = await _categoryRepository.DeleteAsync(id);
            if (deleted)
            {
                await InvalidateCategoryCachesAsync();
            }

            return deleted;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var cachedCategories = await _cacheService.GetAsync<List<CategoryResponse>>(CategoriesCacheKey);
            if (cachedCategories != null)
            {
                _logger.LogInformation("CATEGORY LIST SOURCE | Source: CACHE");
                return cachedCategories;
            }

            _logger.LogInformation("CATEGORY LIST SOURCE | Source: DB");
            var categories = await _categoryRepository.GetAllAsync();
            var orderedCategories = categories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedDate);
            var categoryResponses = _mapper.Map<List<CategoryResponse>>(orderedCategories);
            await _cacheService.SetAsync(CategoriesCacheKey, categoryResponses, CategoriesCacheDuration);
            return categoryResponses;
        }

        public async Task<PaginatedResult<CategoryResponse>> SearchAsync(SearchParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var (categories, totalCount) = await _categoryRepository.SearchAsync(
                parameters.SearchTerm,
                parameters.PageNumber,
                parameters.PageSize);

            var categoryResponses = _mapper.Map<IEnumerable<CategoryResponse>>(categories);

            return new PaginatedResult<CategoryResponse>(
                categoryResponses,
                totalCount,
                parameters.PageNumber,
                parameters.PageSize);
        }

        public async Task<bool> UpdateCategoryOrderAsync(Dictionary<int, int> categoryOrders)
        {
            if (categoryOrders == null || !categoryOrders.Any())
                throw new ArgumentException("Category orders cannot be null or empty", nameof(categoryOrders));

            try
            {
                foreach (var kvp in categoryOrders)
                {
                    var category = await _categoryRepository.GetByIdAsync(kvp.Key);
                    if (category != null)
                    {
                        category.UpdateDisplayOrder(kvp.Value);
                        category.IncrementVersion();
                        await _categoryRepository.UpdateAsync(category, category.Version - 1);
                    }
                }

                await InvalidateCategoryCachesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Task InvalidateCategoryCachesAsync()
        {
            return _cacheService.RemoveManyAsync(new[]
            {
                CategoriesCacheKey,
                ItemService.ActiveItemsCacheKey
            });
        }
    }
}
