using Application.DTOs.Category;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Category -> CategoryResponse
            CreateMap<Category, CategoryResponse>();

            // CreateCategoryRequest -> Category
            // Not needed as we're using the constructor directly in the service
        }
    }
}
