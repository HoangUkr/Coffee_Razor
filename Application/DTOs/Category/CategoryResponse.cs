using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Category
{
    public record CategoryResponse
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int DisplayOrder { get; init; }
        public int Version { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}
