namespace Domain.Entities
{
    public class Holiday
    {
        public int Id { get; private set; }
        public DateOnly Date { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsRecurring { get; private set; }
        public bool IsActive { get; private set; }

        public Holiday(DateOnly date, string name, bool isRecurring = false)
        {
            Date        = date;
            Name        = name.Trim();
            IsRecurring = isRecurring;
            IsActive    = true;
        }

        private Holiday() { }

        public void Update(DateOnly date, string name, bool isRecurring)
        {
            Date        = date;
            Name        = name.Trim();
            IsRecurring = isRecurring;
        }

        public void Deactivate() => IsActive = false;

        public bool MatchesDate(DateOnly target) =>
            IsActive &&
            (Date == target ||
             (IsRecurring && Date.Month == target.Month && Date.Day == target.Day));
    }
}
