namespace Application.DTOs.Settings
{
    public class WorkingScheduleEntry
    {
        public DayOfWeek Day      { get; init; }
        public string    DayName  { get; init; } = string.Empty;
        public TimeOnly  OpenTime  { get; init; }
        public TimeOnly  CloseTime { get; init; }
        public bool      IsClosed  { get; init; }
    }
}
