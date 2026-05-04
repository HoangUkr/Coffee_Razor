using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Category
{
    public record CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category Name should not be empty")]
        public string Name { get; init; }
    }
}
