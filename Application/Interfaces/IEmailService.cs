using Application.DTOs.Email;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(OrderConfirmationEmail email, CancellationToken ct = default);
        Task SendReservationConfirmationAsync(ReservationConfirmationEmail email, CancellationToken ct = default);
    }
}
