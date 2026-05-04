using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs.Item;
using Application.Interfaces;
using Domain.Entities;

namespace Application
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            // Map Item -> ItemResponse using constructor mapping with dynamic SAS URL generation
            CreateMap<Item, ItemResponse>()
                .ConstructUsing((src, context) =>
                {
                    string? imageUrl = null;

                    // Try to get IStorageService from context items (passed during mapping)
                    if (context.Items.TryGetValue("StorageService", out var service) && service is IStorageService storageService)
                    {
                        if (src.ItemImages != null && src.ItemImages.Any())
                        {
                            // Get the default image or first active image
                            var itemImage = src.ItemImages.FirstOrDefault(img => img.IsDefault && img.IsActive)
                                         ?? src.ItemImages.FirstOrDefault(img => img.IsActive)
                                         ?? src.ItemImages.FirstOrDefault();

                            if (itemImage != null && !string.IsNullOrWhiteSpace(itemImage.Url))
                            {
                                // If the URL is already a full URL (legacy data), return as-is
                                if (itemImage.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                    itemImage.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                {
                                    imageUrl = itemImage.Url;
                                }
                                else
                                {
                                    // Otherwise, treat it as a filename and generate dynamic SAS URL
                                    imageUrl = storageService.GetImageUrl(itemImage.Url);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Fallback to original logic if StorageService not available
                        if (src.ItemImages != null && src.ItemImages.Any())
                        {
                            var itemImage = src.ItemImages.FirstOrDefault(img => img.IsDefault)
                                         ?? src.ItemImages.FirstOrDefault();
                            imageUrl = itemImage?.Url;
                        }
                    }

                    return new ItemResponse(
                        src.Id,
                        src.Name,
                        src.Description,
                        src.Price,
                        src.IsActive,
                        src.Version,
                        src.CategoryId,
                        src.Category.Name,
                        imageUrl
                    );
                });

            // DO NOT map CreateItemRequest -> Item directly!
            // Item creation requires Category entity lookup in service layer
            // Use: var item = new Item(request.Name, request.Price, category);
        }
    }
}
