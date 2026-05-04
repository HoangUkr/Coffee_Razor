using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order
{
    public record OrderSummaryResponse(
        int Id,
        string OrderCode,
        decimal TotalPrice,
        int TotalItemsAmount,
        DateTimeOffset CreatedDate
    );
}
