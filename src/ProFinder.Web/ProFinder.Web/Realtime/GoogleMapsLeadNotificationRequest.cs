namespace ProFinder.Web.Realtime;

public class GoogleMapsLeadNotificationRequest
{
    public int? RunId { get; set; }

    public string? SearchQuery { get; set; }

    public string Status { get; set; } = "Running";

    public bool Completed { get; set; }

    public int Captured { get; set; }

    public int Inserted { get; set; }

    public int Updated { get; set; }

    public int Skipped { get; set; }

    public string? Message { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
