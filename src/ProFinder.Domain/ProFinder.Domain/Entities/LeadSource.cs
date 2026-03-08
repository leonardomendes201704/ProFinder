using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class LeadSource : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Professional> Professionals { get; set; } = [];

    public ICollection<LeadCaptureRun> LeadCaptureRuns { get; set; } = [];

    public ICollection<GoogleMapsLead> GoogleMapsLeads { get; set; } = [];

    public ICollection<ProviderLead> ProviderLeads { get; set; } = [];
}
