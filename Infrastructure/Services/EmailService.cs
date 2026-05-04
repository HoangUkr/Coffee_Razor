using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendReservationConfirmationAsync(
            string toEmail,
            string customerName,
            DateTime reservationDate,
            string reservationTime,
            int numberOfGuests)
        {
            // In a real implementation, this would send an actual email using SMTP
            // For now, we'll just log the email content
            var emailContent = $@"
=== RESERVATION CONFIRMATION EMAIL ===
To: {toEmail}
Subject: Your Table Reservation Confirmation

Dear {customerName},

Thank you for choosing our coffee shop! Your table reservation has been confirmed.

Reservation Details:
- Date: {reservationDate:MMMM dd, yyyy}
- Time: {reservationTime}
- Number of Guests: {numberOfGuests}

We look forward to serving you!

Best regards,
Coffee Shop Team
=====================================
";

            _logger.LogInformation(emailContent);
            await Task.CompletedTask;
        }

        public async Task SendReservationCancellationAsync(
            string toEmail,
            string customerName,
            DateTime reservationDate,
            string reservationTime)
        {
            // In a real implementation, this would send an actual email using SMTP
            var emailContent = $@"
=== RESERVATION CANCELLATION EMAIL ===
To: {toEmail}
Subject: Your Table Reservation Cancellation

Dear {customerName},

Your table reservation has been cancelled.

Cancelled Reservation Details:
- Date: {reservationDate:MMMM dd, yyyy}
- Time: {reservationTime}

We hope to see you again soon!

Best regards,
Coffee Shop Team
======================================
";

            _logger.LogInformation(emailContent);
            await Task.CompletedTask;
        }
    }
}
