using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class CreateUserModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<CreateUserModel> _logger;

        public CreateUserModel(
            IUserService userService,
            ILogger<CreateUserModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public CreateUserRequest Input { get; set; } = new CreateUserRequest();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if username already exists
                var isAvailable = await _userService.IsUsernameAvailableAsync(Input.Username);
                if (!isAvailable)
                {
                    ErrorMessage = $"Username '{Input.Username}' is already taken.";
                    return Page();
                }

                var result = await _userService.CreateUserAsync(Input);

                _logger.LogInformation("User created successfully: {Username} (ID: {UserId})", result.Username, result.Id);

                TempData["SuccessMessage"] = $"User '{result.Username}' has been created successfully!";
                return RedirectToPage("/Admin/Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Username}", Input.Username);
                ErrorMessage = "An error occurred while creating the user. Please try again.";
                return Page();
            }
        }
    }
}
