namespace Application.DTOs.Settings
{
    public class HolidayResponse
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public bool IsActive { get; set; }
    }
}
