using Microsoft.AspNetCore.SignalR;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Web.Hubs;

namespace ProFinder.Web.Realtime;

public class CrawlerRunSignalRNotifier : ICrawlerRunRealtimeNotifier
{
    public const string HubRoute = "/hubs/crawler-runs";
    public const string RunUpdatedEventName = "CrawlerRunUpdated";
    public const string LogAddedEventName = "CrawlerRunLogAdded";
    public const string ProviderLeadsChangedEventName = "ProviderLeadsChanged";
    public const string DataResetEventName = "CrawlerDataReset";

    private readonly IHubContext<CrawlerRunsHub> _hubContext;

    public CrawlerRunSignalRNotifier(IHubContext<CrawlerRunsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyRunUpdatedAsync(CrawlerRunDetailsDto run, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(CrawlerRunsHub.BuildGroupName(run.Id))
            .SendAsync(RunUpdatedEventName, run, cancellationToken);

        await _hubContext.Clients.All.SendAsync(
            ProviderLeadsChangedEventName,
            new
            {
                runId = run.Id,
                status = run.Status,
                captured = run.ItemsCaptured,
                inserted = run.ItemsInserted,
                updated = run.ItemsUpdated,
                occurredAtUtc = DateTime.UtcNow
            },
            cancellationToken);
    }

    public Task NotifyLogAddedAsync(CrawlerRunLogDto log, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(CrawlerRunsHub.BuildGroupName(log.LeadCaptureRunId))
            .SendAsync(LogAddedEventName, log, cancellationToken);
    }

    public async Task NotifyDataResetAsync(CrawlerDataResetResultDto result, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync(DataResetEventName, result, cancellationToken);

        await _hubContext.Clients.All.SendAsync(
            ProviderLeadsChangedEventName,
            new
            {
                runId = (int?)null,
                status = "Reset",
                captured = 0,
                inserted = 0,
                updated = 0,
                operation = "reset",
                occurredAtUtc = result.OccurredAtUtc
            },
            cancellationToken);
    }
}
