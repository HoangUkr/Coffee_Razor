using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Reservation
    {
        public int Id { get; private set; }

        [Required]
        [MaxLength(100)]
        public string CustomerName { get; private set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; private set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; private set; } = string.Empty;

        [Required]
        public DateTime ReservationDate { get; private set; }

        [Required]
        [MaxLength(10)]
        public string ReservationTime { get; private set; } = string.Empty;

        [Required]
        public int NumberOfGuests { get; private set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; private set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; private set; } = "Confirmed"; // Pending, Confirmed, Cancelled, Completed

        public int Version { get; private set; }
        public DateTimeOffset CreatedDate { get; private set; }
        public DateTimeOffset? ConfirmedDate { get; private set; }
        public DateTimeOffset? CancelledDate { get; private set; }

        // Constructor for EF Core
        private Reservation() { }

        // Public constructor for creating new reservations
        public Reservation(
            string customerName,
            string phoneNumber,
            DateTime reservationDate,
            string reservationTime,
            int numberOfGuests,
            string? email = null,
            string? specialRequests = null)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                throw new ArgumentException("Customer name is required", nameof(customerName));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(reservationTime))
                throw new ArgumentException("Reservation time is required", nameof(reservationTime));

            if (numberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than 0", nameof(numberOfGuests));

            CustomerName = customerName;
            PhoneNumber = phoneNumber;
            ReservationDate = reservationDate;
            ReservationTime = reservationTime;
            NumberOfGuests = numberOfGuests;
            Email = email;
            SpecialRequests = specialRequests;
            Status = "Confirmed";  // Auto-confirm customer reservations
            ConfirmedDate = DateTimeOffset.UtcNow;
            CreatedDate = DateTimeOffset.UtcNow;
        }

        public void Confirm()
        {
            if (Status == "Cancelled")
                throw new InvalidOperationException("Cannot confirm a cancelled reservation");

            if (Status == "Completed")
                throw new InvalidOperationException("Cannot confirm a completed reservation");

            Status = "Confirmed";
            ConfirmedDate = DateTimeOffset.UtcNow;
        }

        public void Cancel()
        {
            if (Status == "Completed")
                throw new InvalidOperationException("Cannot cancel a completed reservation");

            Status = "Cancelled";
            CancelledDate = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status == "Cancelled")
                throw new InvalidOperationException("Cannot complete a cancelled reservation");

            Status = "Completed";
        }

        public void UpdateDetails(
            string? customerName = null,
            string? phoneNumber = null,
            string? email = null,
            DateTime? reservationDate = null,
            string? reservationTime = null,
            int? numberOfGuests = null,
            string? specialRequests = null)
        {
            if (Status == "Cancelled")
                throw new InvalidOperationException("Cannot update a cancelled reservation");

            if (Status == "Completed")
                throw new InvalidOperationException("Cannot update a completed reservation");

            if (!string.IsNullOrWhiteSpace(customerName))
                CustomerName = customerName;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                PhoneNumber = phoneNumber;

            if (email != null)
                Email = email;

            if (reservationDate.HasValue)
                ReservationDate = reservationDate.Value;

            if (!string.IsNullOrWhiteSpace(reservationTime))
                ReservationTime = reservationTime;

            if (numberOfGuests.HasValue && numberOfGuests.Value > 0)
                NumberOfGuests = numberOfGuests.Value;

            if (specialRequests != null)
                SpecialRequests = specialRequests;
        }

        public void IncrementVersion()
        {
            Version++;
        }
    }
}
