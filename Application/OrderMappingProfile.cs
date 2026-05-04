using AutoMapper;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;

namespace Application
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Map Order -> OrderSummary
            CreateMap<Order, OrderSummaryResponse>()
                .ForMember(dest => dest.TotalItemsAmount, opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)));

            // Map Order -> OrderDetail
            CreateMap<Order, OrderDetailResponse>()
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.CustomerCode))
                .ForMember(dest => dest.CustomerFirstName, opt => opt.MapFrom(src => src.Customer.FirstName))
                .ForMember(dest => dest.CustomerLastName, opt => opt.MapFrom(src => src.Customer.LastName))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => 
                    src.Customer.FirstName + " " + src.Customer.LastName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.PhoneNumber))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            // Map OrderItem -> OrderItemResponse with dynamic SAS URL generation
            CreateMap<OrderItems, OrderItemResponse>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Item.Category.Name))
                .ForMember(dest => dest.ItemImage, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (src.Item?.ItemImages == null || !src.Item.ItemImages.Any())
                    {
                        return null;
                    }

                    var itemImage = src.Item.ItemImages.FirstOrDefault(img => img.IsDefault && img.IsActive)
                                 ?? src.Item.ItemImages.FirstOrDefault(img => img.IsActive)
                                 ?? src.Item.ItemImages.FirstOrDefault();

                    if (itemImage == null || string.IsNullOrWhiteSpace(itemImage.Url))
                    {
                        return null;
                    }

                    // Try to get IStorageService from context items (passed during mapping)
                    if (context.Items.TryGetValue("StorageService", out var service) && service is IStorageService storageService)
                    {
                        // If the URL is already a full URL (legacy data), return as-is
                        if (itemImage.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            itemImage.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            return itemImage.Url;
                        }

                        // Otherwise, treat it as a filename and generate dynamic SAS URL
                        return storageService.GetImageUrl(itemImage.Url);
                    }

                    // Fallback: return the URL as-is
                    return itemImage.Url;
                }));
        }
    }
}

