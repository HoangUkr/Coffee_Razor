using Domain.Entities;

namespace Application.Repositories
{
    public interface IWorkingScheduleRepository
    {
        Task<IEnumerable<WorkingSchedule>> GetAllAsync();
        Task UpsertManyAsync(IEnumerable<WorkingSchedule> schedules);
    }
}
