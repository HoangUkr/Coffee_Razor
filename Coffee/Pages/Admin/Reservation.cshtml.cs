using Application.DTOs.Common;
using Application.DTOs.Reservation;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace WebUI.Pages.Admin
{
    public class ReservationModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationModel> _logger;

        public ReservationModel(
            INotificationService notificationService,
            IReservationService reservationService,
            ILogger<ReservationModel> logger)
        {
            _notificationService = notificationService;
            _reservationService = reservationService;
            _logger = logger;
        }

        public PaginatedResult<ReservationResponse> PaginatedReservations { get; set; } = new();
        public List<ReservationResponse> Reservations => PaginatedReservations.Items.ToList();

        [BindProperty(SupportsGet = true)]
        public DateTime? SelectedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool SortDescending { get; set; } = false;

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [TempData(Key = "AdminSuccessMessage")]
        public string? SuccessMessage { get; set; }

        [TempData(Key = "AdminErrorMessage")]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadReservationsAsync();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int id,
            string customerName,
            string email,
            string phone,
            string date,
            string time,
            int guests,
            int version,
            string? message)
        {
            try
            {
                // Parse date
                DateTime reservationDate;
                if (!DateTime.TryParseExact(date, new[] { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out reservationDate))
                {
                    ErrorMessage = "Invalid date format.";
                    return RedirectToPage(new { 
                        SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                        SearchTerm,
                        PageNumber,
                        PageSize,
                        SortBy,
                        SortDescending,
                        StatusFilter
                    });
                }

                // Normalize time format
                string normalizedTime;
                DateTime parsedTime;
                if (DateTime.TryParse(time, out parsedTime))
                {
                    normalizedTime = parsedTime.ToString("HH:mm");
                }
                else
                {
                    ErrorMessage = "Invalid time format.";
                    return RedirectToPage(new { 
                        SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                        SearchTerm,
                        PageNumber,
                        PageSize,
                        SortBy,
                        SortDescending,
                        StatusFilter
                    });
                }

                var request = new UpdateReservationRequest
                {
                    CustomerName = customerName,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    PhoneNumber = phone,
                    ReservationDate = reservationDate,
                    ReservationTime = normalizedTime,
                    NumberOfGuests = guests,
                    Version = version,
                    SpecialRequests = message
                };

                await _reservationService.UpdateAsync(id, request);
                await _notificationService.CreateForAdminsAsync("Reservation", $"Reservation #{id} updated", "/Admin/Reservation");

                SuccessMessage = "Reservation updated successfully!";

                // Redirect back to the current page with preserved state
                return RedirectToPage(new { 
                    SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                    SearchTerm,
                    PageNumber,
                    PageSize,
                    SortBy,
                    SortDescending,
                    StatusFilter
                });
            }
            catch (ConcurrencyConflictException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while updating reservation {ReservationId}", id);
                ErrorMessage = ex.Message;
                return RedirectToPage(new { 
                    SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                    SearchTerm,
                    PageNumber,
                    PageSize,
                    SortBy,
                    SortDescending,
                    StatusFilter
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while updating reservation {ReservationId}", id);
                ErrorMessage = ex.Message;
                return RedirectToPage(new { 
                    SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                    SearchTerm,
                    PageNumber,
                    PageSize,
                    SortBy,
                    SortDescending,
                    StatusFilter
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation {ReservationId}", id);
                ErrorMessage = "An error occurred while updating the reservation.";
                return RedirectToPage(new { 
                    SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                    SearchTerm,
                    PageNumber,
                    PageSize,
                    SortBy,
                    SortDescending,
                    StatusFilter
                });
            }
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            try
            {
                await _reservationService.ConfirmAsync(id);
                await _notificationService.CreateForAdminsAsync("Reservation", $"Reservation #{id} confirmed", "/Admin/Reservation");
                SuccessMessage = "Reservation confirmed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming reservation {ReservationId}", id);
                ErrorMessage = "An error occurred while confirming the reservation.";
            }

            return RedirectToPage(new { 
                SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                SearchTerm,
                PageNumber,
                PageSize,
                SortBy,
                SortDescending,
                StatusFilter
            });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            try
            {
                await _reservationService.CancelAsync(id);
                await _notificationService.CreateForAdminsAsync("Reservation", $"Reservation #{id} cancelled", "/Admin/Reservation");
                SuccessMessage = "Reservation cancelled successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
                ErrorMessage = "An error occurred while cancelling the reservation.";
            }

            return RedirectToPage(new { 
                SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                SearchTerm,
                PageNumber,
                PageSize,
                SortBy,
                SortDescending,
                StatusFilter
            });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            try
            {
                await _reservationService.CompleteAsync(id);
                await _notificationService.CreateForAdminsAsync("Reservation", $"Reservation #{id} completed", "/Admin/Reservation");
                SuccessMessage = "Reservation marked as completed!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing reservation {ReservationId}", id);
                ErrorMessage = "An error occurred while completing the reservation.";
            }

            return RedirectToPage(new { 
                SelectedDate = SelectedDate?.ToString("yyyy-MM-dd"),
                SearchTerm,
                PageNumber,
                PageSize,
                SortBy,
                SortDescending,
                StatusFilter
            });
        }

        private async Task LoadReservationsAsync()
        {
            // Use the new search method with filters for comprehensive filtering
            PaginatedReservations = await _reservationService.SearchWithFiltersAsync(
                SelectedDate,
                SearchTerm,
                StatusFilter,
                PageNumber,
                PageSize
            );
        }

        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Pending" => "bg-warning",
                "Confirmed" => "bg-success",
                "Cancelled" => "bg-danger",
                "Completed" => "bg-secondary",
                _ => "bg-info"
            };
        }
    }
}
