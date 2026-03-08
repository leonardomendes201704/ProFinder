using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class GoogleMapsLead : BaseEntity, IAuditableEntity
{
    public int LeadCaptureRunId { get; set; }

    public int LeadSourceId { get; set; }

    public int? ProfessionId { get; set; }

    public int? RegionId { get; set; }

    public int? ImportedProfessionalId { get; set; }

    public string SearchQuery { get; set; } = string.Empty;

    public string PlaceUrl { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? NormalizedPhone { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public string ImportStatus { get; set; } = "Captured";

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
