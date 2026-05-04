using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ILogger<ContactModel> _logger;

        public ContactModel(ILogger<ContactModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 1000 characters")]
        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
            _logger.LogInformation("Contact page visited");
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Send email or save to database
            _logger.LogInformation("Contact form submitted by {Name} ({Email}): {Subject}", Name, Email, Subject);

            // For now, just log and show success message
            TempData["SuccessMessage"] = "Thank you for contacting us! We will get back to you soon.";
            
            // Clear form
            ModelState.Clear();
            Name = string.Empty;
            Email = string.Empty;
            Subject = string.Empty;
            Message = string.Empty;

            return RedirectToPage();
        }
    }
}
