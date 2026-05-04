using System;

namespace Application.DTOs.Reservation
{
    public record ReservationResponse(
        int Id,
        string CustomerName,
        string? Email,
        string PhoneNumber,
        DateTime ReservationDate,
        string ReservationTime,
        int NumberOfGuests,
        string? SpecialRequests,
        string Status,
        int Version,
        DateTimeOffset CreatedDate,
        DateTimeOffset? ConfirmedDate,
        DateTimeOffset? CancelledDate
    );
}
