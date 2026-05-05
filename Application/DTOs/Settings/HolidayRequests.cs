using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Settings
{
    public class CreateHolidayRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsRecurring { get; set; }
    }

    public class UpdateHolidayRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsRecurring { get; set; }
    }
}
