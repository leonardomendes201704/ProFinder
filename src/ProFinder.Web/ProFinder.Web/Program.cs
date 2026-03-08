using Microsoft.AspNetCore.Mvc;
using ProFinder.Infrastructure.Extensions;
using ProFinder.Web.Hubs;
using ProFinder.Web.Realtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var realtimeSection = builder.Configuration.GetSection(GoogleMapsLeadRealtimeOptions.SectionName);
builder.Services.AddSingleton(new GoogleMapsLeadRealtimeOptions
{
    GoogleMapsLeadsWebhookKey = realtimeSection["GoogleMapsLeadsWebhookKey"]
});
builder.Services.AddSingleton<GoogleMapsLeadRealtimeNotifier>();
builder.Services.AddSingleton<ProFinder.Application.Interfaces.Services.ICrawlerRunRealtimeNotifier, CrawlerRunSignalRNotifier>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<GoogleMapsLeadsHub>(GoogleMapsLeadRealtimeNotifier.HubRoute);
app.MapHub<CrawlerRunsHub>(CrawlerRunSignalRNotifier.HubRoute);
app.MapPost(
    GoogleMapsLeadRealtimeNotifier.NotifyRoute,
    async (
        [FromBody] GoogleMapsLeadNotificationRequest request,
        HttpRequest httpRequest,
        GoogleMapsLeadRealtimeOptions options,
        GoogleMapsLeadRealtimeNotifier notifier,
        CancellationToken cancellationToken) =>
    {
        if (!string.IsNullOrWhiteSpace(options.GoogleMapsLeadsWebhookKey))
        {
            var incomingKey = httpRequest.Headers["X-Webhook-Key"].FirstOrDefault();
            if (!string.Equals(incomingKey, options.GoogleMapsLeadsWebhookKey, StringComparison.Ordinal))
            {
                return Results.Unauthorized();
            }
        }

        request.Status = string.IsNullOrWhiteSpace(request.Status) ? "Running" : request.Status.Trim();
        request.Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();
        if (request.OccurredAtUtc == default)
        {
            request.OccurredAtUtc = DateTime.UtcNow;
        }

        await notifier.NotifyAsync(request, cancellationToken);
        return Results.Accepted();
    });

app.Run();

public partial class Program;
