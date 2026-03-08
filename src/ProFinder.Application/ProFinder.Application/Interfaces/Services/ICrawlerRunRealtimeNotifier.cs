using ProFinder.Application.DTOs.Capture;

namespace ProFinder.Application.Interfaces.Services;

public interface ICrawlerRunRealtimeNotifier
{
    Task NotifyRunUpdatedAsync(CrawlerRunDetailsDto run, CancellationToken cancellationToken = default);

    Task NotifyLogAddedAsync(CrawlerRunLogDto log, CancellationToken cancellationToken = default);

    Task NotifyDataResetAsync(CrawlerDataResetResultDto result, CancellationToken cancellationToken = default);
}
