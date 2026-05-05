using Application.DTOs.Email;
using Application.Interfaces;
using Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        // Cache templates in memory after first load — they never change at runtime
        private static readonly Dictionary<string, string> _templateCache = new();
        private static readonly object _cacheLock = new();

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        // ── Public methods ────────────────────────────────────────────────────

        public async Task SendOrderConfirmationAsync(OrderConfirmationEmail email, CancellationToken ct = default)
        {
            var template = LoadTemplate("order_confirmation.html");

            var itemRows = BuildOrderItemRows(email.Items);
            var deliveryAddressRow = BuildDeliveryAddressRow(email.DeliveryAddress);

            var body = template
                .Replace("{{ customer_name }}", email.CustomerName)
                .Replace("{{ order_code }}", email.OrderCode)
                .Replace("{{ order_date }}", email.CreatedDate.ToLocalTime().ToString("MMMM dd, yyyy HH:mm"))
                .Replace("{{ fulfillment_type }}", email.FulfillmentDescription)
                .Replace("{{ delivery_address_row }}", deliveryAddressRow)
                .Replace("{{ order_items }}", itemRows)
                .Replace("{{ total_price }}", email.TotalPrice.ToString("C"))
                .Replace("{{ notes }}", string.IsNullOrWhiteSpace(email.Notes) ? "—" : email.Notes);

            await SendAsync(email.ToEmail, email.CustomerName, $"Order #{email.OrderCode} Confirmed", body, ct);
        }

        public async Task SendReservationConfirmationAsync(ReservationConfirmationEmail email, CancellationToken ct = default)
        {
            var template = LoadTemplate("reservation_confirmation.html");

            var body = template
                .Replace("{{ customer_name }}", email.CustomerName)
                .Replace("{{ reservation_id }}", email.ReservationId.ToString())
                .Replace("{{ number_of_people }}", email.NumberOfGuests.ToString())
                .Replace("{{ customer_email }}", email.ToEmail)
                .Replace("{{ customer_phone }}", email.PhoneNumber)
                .Replace("{{ reservation_date }}", email.ReservationDate.ToString("MMMM dd, yyyy") + " at " + email.ReservationTime)
                .Replace("{{ customer_message }}", string.IsNullOrWhiteSpace(email.SpecialRequests) ? "—" : email.SpecialRequests);

            await SendAsync(email.ToEmail, email.CustomerName, "Table Reservation Confirmed", body, ct);
        }

        // ── Core SMTP sender — single place for all transport logic ──────────

        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent | To: {Email} | Subject: {Subject}", toEmail, subject);
        }

        // ── Template loading — reads from embedded resources, cached after first load ──

        private static string LoadTemplate(string fileName)
        {
            lock (_cacheLock)
            {
                if (_templateCache.TryGetValue(fileName, out var cached))
                    return cached;

                var assembly = typeof(EmailService).Assembly;
                var resourceName = $"Infrastructure.EmailTemplates.{fileName}";

                using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new FileNotFoundException($"Email template '{fileName}' not found as embedded resource '{resourceName}'.");

                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                _templateCache[fileName] = content;
                return content;
            }
        }

        // ── HTML fragment builders ────────────────────────────────────────────

        private static string BuildOrderItemRows(IEnumerable<OrderItemEmailLine> items)
        {
            const string cellStyle = "vertical-align:top;padding:10px;word-break:break-word;border:1px solid #dddddd;";

            return string.Concat(items.Select(i => $"""
                <tr>
                    <td style="{cellStyle}">{i.ItemName}</td>
                    <td style="{cellStyle}text-align:center;">{i.Quantity}</td>
                    <td style="{cellStyle}text-align:right;">{i.LineTotal:C}</td>
                </tr>
                """));
        }

        private static string BuildDeliveryAddressRow(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return string.Empty;

            const string cellStyle = "vertical-align:top;padding:10px;word-break:break-word;border:1px solid #dddddd;";
            return $"""
                <tr>
                    <td width="50%" style="{cellStyle}">Delivery Address</td>
                    <td width="50%" style="{cellStyle}">{address}</td>
                </tr>
                """;
        }
    }
}

