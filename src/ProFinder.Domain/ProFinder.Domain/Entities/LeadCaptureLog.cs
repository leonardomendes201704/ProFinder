using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class LeadCaptureLog : BaseEntity, IAuditableEntity
{
    public int LeadCaptureRunId { get; set; }

    public string LogLevel { get; set; } = "Info";

    public string Source { get; set; } = "process";

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public LeadCaptureRun? LeadCaptureRun { get; set; }
}
