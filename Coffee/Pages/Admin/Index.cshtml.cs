using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)
                    ? Url.Content("~/Admin/Inventory")!
                    : returnUrl);
            }

            return RedirectToPage("/Admin/Login", new { returnUrl = NormalizeReturnUrl(returnUrl) });
        }

        private string NormalizeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return Url.Content("~/Admin/Inventory")!;
        }
    }
}
