using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Settings
{
    public class UpdateScheduleDayRequest
    {
        public DayOfWeek Day      { get; set; }
        public bool      IsClosed { get; set; }

        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Use HH:mm format")]
        public string OpenTime  { get; set; } = "08:00";

        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Use HH:mm format")]
        public string CloseTime { get; set; } = "22:00";
    }
}
