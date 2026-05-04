using Application.DTOs.User;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(
            IUserService userService,
            ILogger<UsersModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public PaginatedResult<UserResponse> Users { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        [BindProperty]
        public UpdateUserDetailsRequest? EditInput { get; set; }

        [BindProperty]
        public Guid EditUserId { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();

            // Check for success message from TempData
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid || EditInput == null)
            {
                ErrorMessage = "Invalid user data.";
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                var result = await _userService.UpdateUserDetailsAsync(EditUserId, EditInput);
                
                if (result != null)
                {
                    _logger.LogInformation("User updated successfully: {Username} (ID: {UserId})", result.Username, result.Id);
                    TempData["SuccessMessage"] = $"User '{result.Username}' has been updated successfully!";
                    return RedirectToPage();
                }
                else
                {
                    ErrorMessage = "User not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating user: {UserId}", EditUserId);
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", EditUserId);
                ErrorMessage = "An error occurred while updating the user. Please try again.";
            }

            await LoadUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToPage();
                }

                var result = await _userService.DeleteUserAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("User deleted successfully: {Username} (ID: {UserId})", user.Username, id);
                    TempData["SuccessMessage"] = $"User '{user.Username}' has been deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not found." });
                }

                bool result;
                if (user.IsActive)
                {
                    result = await _userService.DeactivateAccountAsync(id);
                    if (result)
                    {
                        _logger.LogInformation("User deactivated: {Username} (ID: {UserId})", user.Username, id);
                        return new JsonResult(new { success = true, message = "User deactivated successfully.", isActive = false });
                    }
                }
                else
                {
                    result = await _userService.ActivateUserAsync(id);
                    if (result)
                    {
                        _logger.LogInformation("User activated: {Username} (ID: {UserId})", user.Username, id);
                        return new JsonResult(new { success = true, message = "User activated successfully.", isActive = true });
                    }
                }

                return new JsonResult(new { success = false, message = "Failed to update user status." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status: {UserId}", id);
                return new JsonResult(new { success = false, message = "An error occurred while updating user status." });
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var parameters = new SearchParameters
                {
                    SearchTerm = SearchTerm,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                Users = await _userService.SearchAsync(parameters, includeInactive: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                Users = new PaginatedResult<UserResponse>();
                ErrorMessage = "An error occurred while loading users.";
            }
        }
    }
}
