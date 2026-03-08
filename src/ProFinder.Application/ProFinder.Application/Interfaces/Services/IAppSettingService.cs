using ProFinder.Application.DTOs.Settings;

namespace ProFinder.Application.Interfaces.Services;

public interface IAppSettingService
{
    Task<IReadOnlyList<AppSettingGroupDto>> GetGroupedAsync(CancellationToken cancellationToken = default);

    Task<AppSettingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpdateAppSettingDto dto, CancellationToken cancellationToken = default);
}
