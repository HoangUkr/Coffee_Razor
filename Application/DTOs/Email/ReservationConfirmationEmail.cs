namespace Application.DTOs.Email
{
    public record ReservationConfirmationEmail
    {
        public string ToEmail { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public int ReservationId { get; init; }
        public DateTime ReservationDate { get; init; }
        public string ReservationTime { get; init; } = string.Empty;
        public int NumberOfGuests { get; init; }
        public string? SpecialRequests { get; init; }
    }
}
