namespace Application.DTOs.Settings
{
    public class AppSettings
    {
        public string ContactEmail     { get; init; } = string.Empty;
        public string ContactPhone     { get; init; } = string.Empty;
        public string ContactAddress   { get; init; } = string.Empty;
        public string ContactFacebook  { get; init; } = string.Empty;
        public string ContactInstagram { get; init; } = string.Empty;
        public string ContactTwitter   { get; init; } = string.Empty;

        public bool EmailConfirmationEnabled { get; init; } = true;
        public bool ShowNotificationCount    { get; init; } = true;
    }
}
