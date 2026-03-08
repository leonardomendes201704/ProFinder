using Microsoft.AspNetCore.SignalR;
using ProFinder.Web.Hubs;

namespace ProFinder.Web.Realtime;

public class GoogleMapsLeadRealtimeNotifier
{
    public const string EventName = "GoogleMapsLeadBatchUpdated";
    public const string HubRoute = "/hubs/google-maps-leads";
    public const string NotifyRoute = "/integrations/google-maps-leads/notify";

    private readonly IHubContext<GoogleMapsLeadsHub> _hubContext;

    public GoogleMapsLeadRealtimeNotifier(IHubContext<GoogleMapsLeadsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyAsync(GoogleMapsLeadNotificationRequest request, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(EventName, request, cancellationToken);
    }
}
