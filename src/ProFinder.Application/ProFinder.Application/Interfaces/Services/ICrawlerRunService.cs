using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Filters;

namespace ProFinder.Application.Interfaces.Services;

public interface ICrawlerRunService
{
    Task<PagedResult<CrawlerRunListItemDto>> GetPagedAsync(CrawlerRunQueryFilter filter, CancellationToken cancellationToken = default);

    Task<CrawlerRunDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CrawlerRunLogDto>> GetLogsAsync(int runId, int take = 200, CancellationToken cancellationToken = default);

    Task<CrawlerLauncherDefaultsDto> GetLauncherDefaultsAsync(CancellationToken cancellationToken = default);

    Task<LeadCaptureRunDto> QueueAsync(StartCrawlerRunRequestDto dto, CancellationToken cancellationToken = default);

    Task<LeadCaptureRunDto> RequestStopAsync(int runId, string requestedBy, CancellationToken cancellationToken = default);

    Task<CrawlerDataResetResultDto> ResetDataAsync(string requestedBy, CancellationToken cancellationToken = default);
}
