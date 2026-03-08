using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class ProviderLead : BaseEntity, IAuditableEntity
{
    public int LeadCaptureRunId { get; set; }

    public int LeadSourceId { get; set; }

    public int? ProfessionId { get; set; }

    public int? RegionId { get; set; }

    public int? ImportedProfessionalId { get; set; }

    public string SiteKey { get; set; } = string.Empty;

    public string SearchQuery { get; set; } = string.Empty;

    public string DeduplicationKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? WhatsApp { get; set; }

    public string? NormalizedPhone { get; set; }

    public string? Address { get; set; }

    public string? Neighborhood { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Website { get; set; }

    public string? SourceListingUrl { get; set; }

    public string? SourceDetailsUrl { get; set; }

    public string? ExternalId { get; set; }

    public string ImportStatus { get; set; } = "Captured";

    public string? SourceSitesJson { get; set; }

    public string? SourceUrlsJson { get; set; }

    public int SourceCount { get; set; } = 1;

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public string? RawPayloadJson { get; set; }

    public DateTime ScrapedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public LeadCaptureRun? LeadCaptureRun { get; set; }

    public LeadSource? LeadSource { get; set; }

    public Profession? Profession { get; set; }

    public Region? Region { get; set; }

    public Professional? ImportedProfessional { get; set; }
}
