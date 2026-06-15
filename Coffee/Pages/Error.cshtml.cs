using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages
{
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public void OnGet()
        {
            RequestId = HttpContext.TraceIdentifier;
        }
    }
}
