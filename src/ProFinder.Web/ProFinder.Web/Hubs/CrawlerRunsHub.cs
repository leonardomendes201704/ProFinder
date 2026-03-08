using Microsoft.AspNetCore.SignalR;

namespace ProFinder.Web.Hubs;

public class CrawlerRunsHub : Hub
{
    public Task JoinRunGroup(int runId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(runId));
    }

    public Task LeaveRunGroup(int runId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildGroupName(runId));
    }

    public static string BuildGroupName(int runId) => $"crawler-run-{runId}";
}
