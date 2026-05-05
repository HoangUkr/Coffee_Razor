using Application.DTOs.Settings;

namespace Application.Interfaces
{
    public interface ISystemSettingService
    {
        Task<AppSettings> GetAppSettingsAsync();
        Task UpdateAsync(UpdateSettingsRequest request);
    }
}
