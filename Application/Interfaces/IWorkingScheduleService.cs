using Application.DTOs.Settings;

namespace Application.Interfaces
{
    public interface IWorkingScheduleService
    {
        Task<IReadOnlyList<WorkingScheduleEntry>> GetScheduleAsync();
        Task UpdateScheduleAsync(IEnumerable<UpdateScheduleDayRequest> requests);
    }
}
