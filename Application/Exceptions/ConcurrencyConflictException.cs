namespace Application.Exceptions
{
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(string message)
            : base(message)
        {
        }
    }
}
