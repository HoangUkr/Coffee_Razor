using Application.DTOs.Settings;

namespace Application.Interfaces
{
    public interface IHolidayService
    {
        Task<IReadOnlyList<HolidayResponse>> GetAllActiveAsync();
        Task<HolidayResponse?> GetByIdAsync(int id);
        Task<HolidayResponse?> GetHolidayForDateAsync(DateOnly date);
        Task<int> CreateAsync(CreateHolidayRequest request);
        Task UpdateAsync(int id, UpdateHolidayRequest request);
        Task DeleteAsync(int id);
    }
}
