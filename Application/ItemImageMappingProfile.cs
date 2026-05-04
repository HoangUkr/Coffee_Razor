using Application.DTOs.ItemImages;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class ItemImageMappingProfile : Profile
    {
        public ItemImageMappingProfile()
        {
            // ItemImages -> ItemImageResponse
            CreateMap<ItemImages, ItemImageResponse>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name));
        }
    }
}
