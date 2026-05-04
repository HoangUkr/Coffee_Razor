using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Item
{
    public record ItemResponse(
        int Id,
        string Name,
        string Description,
        decimal Price,
        bool IsActive,
        int Version,
        int CategoryId,
        string CategoryName,
        string? ImageUrl
    );
}
