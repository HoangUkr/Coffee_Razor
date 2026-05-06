using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebUI.Pages.Admin.Settings
{
    public class ScheduleModel : PageModel
    {
        private readonly IWorkingScheduleService _scheduleService;
        private readonly IHolidayService _holidayService;
        private readonly ILogger<ScheduleModel> _logger;

        public ScheduleModel(
            IWorkingScheduleService scheduleService,
            IHolidayService holidayService,
            ILogger<ScheduleModel> logger)
        {
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
            _holidayService  = holidayService  ?? throw new ArgumentNullException(nameof(holidayService));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        [TempData(Key = "ScheduleSuccessMessage")]
        public string? SuccessMessage { get; set; }

        [TempData(Key = "ScheduleErrorMessage")]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public List<UpdateScheduleDayRequest> Schedule { get; set; } = new();

        public CreateHolidayRequest NewHoliday { get; set; } = new();

        public IReadOnlyList<HolidayResponse> Holidays { get; set; } = Array.Empty<HolidayResponse>();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        // Save weekly schedule
        public async Task<IActionResult> OnPostSaveScheduleAsync()
        {
            // Restore safe defaults for closed days so TimeOnly.Parse never fails
            for (int i = 0; i < Schedule.Count; i++)
            {
                if (Schedule[i].IsClosed)
                {
                    ModelState.Remove($"Schedule[{i}].OpenTime");
                    ModelState.Remove($"Schedule[{i}].CloseTime");
                    Schedule[i].OpenTime  = "08:00";
                    Schedule[i].CloseTime = "22:00";
                }
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => $"{e.Key}: {e.Value!.Errors[0].ErrorMessage}");
                _logger.LogWarning("SaveSchedule ModelState invalid: {Errors}", string.Join("; ", errors));
                ErrorMessage = "Could not save schedule. Please check the time values and try again.";
                await LoadAsync();
                return Page();
            }

            try
            {
                await _scheduleService.UpdateScheduleAsync(Schedule);
                SuccessMessage = "Working hours saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving working schedule.");
                ErrorMessage = "An error occurred while saving the schedule.";
            }

            return RedirectToPage();
        }

        // Add a holiday
        public async Task<IActionResult> OnPostAddHolidayAsync()
        {
            // Bind and validate only NewHoliday for this handler
            NewHoliday = new CreateHolidayRequest();
            if (!await TryUpdateModelAsync(NewHoliday, "NewHoliday") || !ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            try
            {
                await _holidayService.CreateAsync(NewHoliday);
                SuccessMessage = $"Holiday \"{NewHoliday.Name}\" added successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding holiday.");
                ErrorMessage = "An error occurred while adding the holiday.";
            }

            return RedirectToPage();
        }

        // Delete a holiday
        public async Task<IActionResult> OnPostDeleteHolidayAsync(int id)
        {
            try
            {
                await _holidayService.DeleteAsync(id);
                SuccessMessage = "Holiday removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting holiday {Id}.", id);
                ErrorMessage = "An error occurred while removing the holiday.";
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            var schedule = await _scheduleService.GetScheduleAsync();
            Schedule = schedule.Select(s => new UpdateScheduleDayRequest
            {
                Day       = s.Day,
                OpenTime  = s.OpenTime.ToString("HH:mm"),
                CloseTime = s.CloseTime.ToString("HH:mm"),
                IsClosed  = s.IsClosed,
            }).ToList();

            Holidays = await _holidayService.GetAllActiveAsync();
        }
    }
}
