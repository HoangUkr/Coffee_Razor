using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reservation
{
    public class UpdateReservationRequest
    {
        [Required]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time must be in HH:mm format")]
        public string ReservationTime { get; set; } = string.Empty;

        [Required]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; } = 2;

        public int Version { get; set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }
    }
}
