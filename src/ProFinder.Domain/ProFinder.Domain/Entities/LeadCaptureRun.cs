using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class LeadCaptureRun : BaseEntity, IAuditableEntity
{
    public int? LeadSourceId { get; set; }

    public string CaptureType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public string? FileName { get; set; }

    public string? SearchQuery { get; set; }

    public string? Notes { get; set; }

    public string? ErrorMessage { get; set; }

    public string? CreatedBy { get; set; }

    public int ItemsCaptured { get; set; }

    public int ItemsInserted { get; set; }

    public int ItemsUpdated { get; set; }

    public int ItemsSkipped { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public LeadSource? LeadSource { get; set; }

    public ICollection<GoogleMapsLead> GoogleMapsLeads { get; set; } = [];

    public ICollection<ProviderLead> ProviderLeads { get; set; } = [];

    public ICollection<LeadCaptureLog> Logs { get; set; } = [];
}
