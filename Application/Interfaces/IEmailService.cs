namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendReservationConfirmationAsync(string toEmail, string customerName, DateTime reservationDate, string reservationTime, int numberOfGuests);
        Task SendReservationCancellationAsync(string toEmail, string customerName, DateTime reservationDate, string reservationTime);
    }
}
