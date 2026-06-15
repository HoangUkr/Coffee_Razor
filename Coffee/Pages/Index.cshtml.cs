using Application.DTOs.Item;
using Application.DTOs.Reservation;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IItemService _itemService;
        private readonly IReservationService _reservationService;
        private readonly ISystemSettingService _settingService;
        private readonly IWorkingScheduleService _scheduleService;
        private readonly IHolidayService _holidayService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IItemService itemService,
            IReservationService reservationService,
            ISystemSettingService settingService,
            IWorkingScheduleService scheduleService,
            IHolidayService holidayService,
            ILogger<IndexModel> logger)
        {
            _itemService        = itemService;
            _reservationService = reservationService;
            _settingService     = settingService;
            _scheduleService    = scheduleService;
            _holidayService     = holidayService;
            _logger             = logger;
        }

        public List<ItemWithImageResponse> BestSellers { get; set; } = new();
        public AppSettings Settings { get; set; } = new();
        public IReadOnlyList<WorkingScheduleEntry> Schedule { get; set; } = Array.Empty<WorkingScheduleEntry>();

        /// <summary>Null = open; otherwise the display label e.g. "Closed – Christmas Day"</summary>
        public string? TodayClosedReason { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public string? InlineErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var items = await _itemService.GetAllActiveAsync();
            BestSellers = items.Take(4).Select(item => new ItemWithImageResponse
            {
                Id              = item.Id,
                Name            = item.Name,
                Price           = item.Price,
                CategoryName    = item.CategoryName,
                DefaultImageUrl = string.IsNullOrEmpty(item.ImageUrl) ? "~/images/menu-1.jpg" : item.ImageUrl
            }).ToList();

            Settings = await _settingService.GetAppSettingsAsync();
            Schedule = await _scheduleService.GetScheduleAsync();

            // Holiday overrides weekly schedule
            var today   = DateOnly.FromDateTime(DateTime.Today);
            var holiday = await _holidayService.GetHolidayForDateAsync(today);
            if (holiday is not null)
            {
                TodayClosedReason = holiday.IsRecurring
                    ? $"Closed – {holiday.Name}"
                    : $"Closed for {holiday.Name}";
            }
        }

        public async Task<IActionResult> OnPostBookTableAsync(
            string firstName, 
            string lastName, 
            string date, 
            string time, 
            string phone, 
            string? message,
            string? email)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    InlineErrorMessage = "Please provide your name.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    InlineErrorMessage = "Please provide your phone number.";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                {
                    InlineErrorMessage = "Please select date and time.";
                    return Page();
                }

                // Log received values for debugging
                _logger.LogInformation("Received booking request - Date: '{Date}', Time: '{Time}'", date, time);

                // Parse date and time
                DateTime reservationDate;
                if (!DateTime.TryParseExact(date, new[] { "MM/dd/yyyy", "M/d/yyyy", "yyyy-MM-dd" }, 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out reservationDate))
                {
                    InlineErrorMessage = "Invalid date format. Please use MM/DD/YYYY format.";
                    return Page();
                }

                _logger.LogInformation(
                    "Parsed date - ReservationDate: {ReservationDate} ({ReservationDateDate}), ReservationDate.Date: {ReservationDateDate2}, DateTime.Today: {Today}, DateTime.Now: {Now}, DateTime.UtcNow: {UtcNow}",
                    reservationDate,
                    reservationDate.Date,
                    reservationDate.Date.ToString("yyyy-MM-dd"),
                    DateTime.Today.ToString("yyyy-MM-dd"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                // Validate date is not in the past
                if (reservationDate.Date < DateTime.Today)
                {
                    _logger.LogWarning(
                        "Attempted to book in the past - ReservationDate: {ReservationDate}, Today: {Today}", 
                        reservationDate.Date.ToString("yyyy-MM-dd"), 
                        DateTime.Today.ToString("yyyy-MM-dd"));
                    InlineErrorMessage = $"Cannot create reservation for a past date. You selected {reservationDate:MMMM dd, yyyy}, but today is {DateTime.Today:MMMM dd, yyyy}.";
                    return Page();
                }

                _logger.LogInformation(
                    "Date validation passed - ReservationDate: {ReservationDate}, Today: {Today}", 
                    reservationDate.Date.ToString("yyyy-MM-dd"), 
                    DateTime.Today.ToString("yyyy-MM-dd"));

                // Normalize time format - convert various formats to HH:mm
                string normalizedTime;
                try
                {
                    // Try to parse the time string to DateTime
                    DateTime parsedTime;
                    if (DateTime.TryParse(time, out parsedTime))
                    {
                        normalizedTime = parsedTime.ToString("HH:mm");
                    }
                    else if (DateTime.TryParseExact(time, new[] { "h:i tt", "h:mm tt", "HH:mm", "H:mm" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                    {
                        normalizedTime = parsedTime.ToString("HH:mm");
                    }
                    else
                    {
                        InlineErrorMessage = $"Invalid time format: '{time}'. Please select a valid time.";
                        return Page();
                    }

                    _logger.LogInformation("Normalized time from '{OriginalTime}' to '{NormalizedTime}'", time, normalizedTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing time: {Time}", time);
                    InlineErrorMessage = $"Invalid time format: '{time}'. Please select a valid time.";
                    return Page();
                }

                // Create reservation request
                var request = new CreateReservationRequest
                {
                    CustomerName = $"{firstName} {lastName}".Trim(),
                    Email = email,
                    PhoneNumber = phone,
                    ReservationDate = reservationDate,
                    ReservationTime = normalizedTime,
                    NumberOfGuests = int.TryParse(Request.Form["guests"], out var guests) ? guests : 2,
                    SpecialRequests = message
                };

                // Create reservation
                var result = await _reservationService.CreateAsync(request);

                _logger.LogInformation(
                    "Reservation created from home page: {CustomerName} for {Date} at {Time}",
                    result.CustomerName,
                    result.ReservationDate.ToString("yyyy-MM-dd"),
                    result.ReservationTime);

                // Set success message
                SuccessMessage = $"Thank you! Your table has been reserved for {result.ReservationDate:MMMM dd, yyyy} at {result.ReservationTime}.";

                if (!string.IsNullOrWhiteSpace(result.Email))
                {
                    SuccessMessage += " A confirmation email has been sent.";
                }

                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating reservation from home page");
                InlineErrorMessage = ex.Message;
                await OnGetAsync(); // Reload data for page display
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation from home page");
                InlineErrorMessage = $"An error occurred while booking your table: {ex.Message}";
                await OnGetAsync(); // Reload data for page display
                return Page();
            }
        }
    }

    // Helper class for displaying items with images
    public class ItemWithImageResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string DefaultImageUrl { get; set; } = string.Empty;
    }
}
