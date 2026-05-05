namespace Domain.Entities
{
    public class WorkingSchedule
    {
        public DayOfWeek Day { get; private set; }
        public TimeOnly OpenTime { get; private set; }
        public TimeOnly CloseTime { get; private set; }
        public bool IsClosed { get; private set; }

        public WorkingSchedule(DayOfWeek day, TimeOnly openTime, TimeOnly closeTime, bool isClosed = false)
        {
            Day       = day;
            OpenTime  = openTime;
            CloseTime = closeTime;
            IsClosed  = isClosed;
        }

        private WorkingSchedule() { }

        public void Update(TimeOnly openTime, TimeOnly closeTime, bool isClosed)
        {
            OpenTime  = openTime;
            CloseTime = closeTime;
            IsClosed  = isClosed;
        }
    }
}
