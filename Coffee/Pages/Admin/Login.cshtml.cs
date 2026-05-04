using Application.DTOs.User;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private const string AuthTokenCookieName = "AuthToken";
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IUserService _userService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ITokenBlacklistService tokenBlacklistService, IUserService userService, ILogger<LoginModel> logger)
        {
            _tokenBlacklistService = tokenBlacklistService;
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public LoginRequest Input { get; set; } = new LoginRequest();

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(Url.Content("~/Admin/Inventory"));
            }

            ReturnUrl = NormalizeReturnUrl(returnUrl);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = NormalizeReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _userService.LoginAsync(Input);

                if (!string.Equals(result.User.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Non-admin user {Username} attempted to access the admin login page", result.User.Username);
                    ErrorMessage = "This page allow only admin.";
                    return Page();
                }

                Response.Cookies.Append(AuthTokenCookieName, result.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.Expiry.UtcDateTime,
                    Path = "/"
                });

                _logger.LogInformation("User {Username} logged in successfully", result.User.Username);

                return LocalRedirect(ReturnUrl ?? Url.Content("~/Admin/Inventory"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Invalid login attempt for username: {Username}", Input.Username);
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", Input.Username);
                ErrorMessage = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLogout(string? returnUrl = null)
        {
            if (Request.Cookies.TryGetValue(AuthTokenCookieName, out var token) && !string.IsNullOrWhiteSpace(token))
            {
                await _tokenBlacklistService.BlacklistTokenAsync(token);
            }

            Response.Cookies.Delete(AuthTokenCookieName, new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Strict,
                HttpOnly = true
            });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Admin/Index");
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
