using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Infrastructure.Services;

public class NoOpCrawlerRunRealtimeNotifier : ICrawlerRunRealtimeNotifier
{
    public Task NotifyRunUpdatedAsync(CrawlerRunDetailsDto run, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyLogAddedAsync(CrawlerRunLogDto log, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyDataResetAsync(CrawlerDataResetResultDto result, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
