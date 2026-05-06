using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Settings
{
    public class UpdateSettingsRequest
    {
        [EmailAddress]
        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [Phone]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(300)]
        public string? ContactAddress { get; set; }

        [MaxLength(500)]
        public string? ContactFacebook { get; set; }

        [MaxLength(500)]
        public string? ContactInstagram { get; set; }

        [MaxLength(500)]
        public string? ContactTwitter { get; set; }

        public bool EmailConfirmationEnabled { get; set; } = true;
        public bool ShowNotificationCount    { get; set; } = true;
    }
}
