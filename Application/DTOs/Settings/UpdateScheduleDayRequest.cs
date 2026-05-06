using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Settings
{
    public class UpdateScheduleDayRequest
    {
        public DayOfWeek Day      { get; set; }
        public bool      IsClosed { get; set; }

        public string OpenTime  { get; set; } = "08:00";

        public string CloseTime { get; set; } = "22:00";
    }
}
