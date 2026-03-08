namespace ProFinder.Application.DTOs.ProviderLeads;

public class ProviderLeadListItemDto
{
    public int Id { get; set; }

    public int LeadCaptureRunId { get; set; }

    public int LeadSourceId { get; set; }

    public string SiteKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? WhatsApp { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Website { get; set; }

    public string? ProfessionName { get; set; }

    public string? RegionDisplayName { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public string ImportStatus { get; set; } = string.Empty;

    public int SourceCount { get; set; }

    public DateTime ScrapedAt { get; set; }
}

public class ProviderLeadDetailsDto
{
    public int Id { get; set; }

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

    public string ImportStatus { get; set; } = string.Empty;

    public string? SourceSitesJson { get; set; }

    public string? SourceUrlsJson { get; set; }

    public int SourceCount { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public string? RawPayloadJson { get; set; }

    public string? ProfessionName { get; set; }

    public string? RegionDisplayName { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public string? ImportedProfessionalName { get; set; }

    public DateTime ScrapedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
