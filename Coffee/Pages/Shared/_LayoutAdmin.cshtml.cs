using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Shared
{
    public class _LayoutAdminModel : PageModel
    {
        private readonly ILogger<_LayoutAdminModel> _logger;

        public _LayoutAdminModel(ILogger<_LayoutAdminModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("Admin user logged out successfully");
                return RedirectToPage("/Admin/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin logout");
                return RedirectToPage("/Admin/Login");
            }
        }
    }
}
