using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin.Settings
{
    public class OthersModel : PageModel
    {
        private readonly ISystemSettingService _settingService;
        private readonly ILogger<OthersModel> _logger;

        public OthersModel(ISystemSettingService settingService, ILogger<OthersModel> logger)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
        }

        [TempData(Key = "OthersSuccessMessage")]
        public string? SuccessMessage { get; set; }

        [TempData(Key = "OthersErrorMessage")]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public UpdateSettingsRequest Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            var settings = await _settingService.GetAppSettingsAsync();
            Input = new UpdateSettingsRequest
            {
                ContactEmail             = settings.ContactEmail,
                ContactFacebook          = settings.ContactFacebook,
                ContactInstagram         = settings.ContactInstagram,
                ContactTwitter           = settings.ContactTwitter,
                EmailConfirmationEnabled = settings.EmailConfirmationEnabled,
                ShowNotificationCount    = settings.ShowNotificationCount,
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                await _settingService.UpdateAsync(Input);
                SuccessMessage = "Settings saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving system settings.");
                ErrorMessage = "An error occurred while saving settings. Please try again.";
            }

            return RedirectToPage();
        }
    }
}
