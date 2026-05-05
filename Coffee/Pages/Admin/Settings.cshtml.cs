using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    // Redirect root /Admin/Settings to /Admin/Settings/Schedule
    public class SettingsModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Admin/Settings/Schedule");
    }
}
