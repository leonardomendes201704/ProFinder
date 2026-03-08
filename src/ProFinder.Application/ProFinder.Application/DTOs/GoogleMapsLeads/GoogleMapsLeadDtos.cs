namespace ProFinder.Application.DTOs.GoogleMapsLeads;

public class GoogleMapsLeadListItemDto
{
    public int Id { get; set; }

    public int LeadCaptureRunId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public string SearchQuery { get; set; } = string.Empty;

    public string ImportStatus { get; set; } = string.Empty;

    public string? ProfessionName { get; set; }

    public string? RegionDisplayName { get; set; }

    public DateTime ScrapedAt { get; set; }
}

public class GoogleMapsLeadDetailsDto
{
    public int Id { get; set; }

    public int LeadCaptureRunId { get; set; }

    public int LeadSourceId { get; set; }

    public int? ProfessionId { get; set; }

    public int? RegionId { get; set; }

    public int? ImportedProfessionalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SearchQuery { get; set; } = string.Empty;

    public string PlaceUrl { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? NormalizedPhone { get; set; }

    public string? Address { get; set; }

    public string? Website { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public string ImportStatus { get; set; } = string.Empty;

    public string? RawPayloadJson { get; set; }

    public string? ProfessionName { get; set; }

    public string? RegionDisplayName { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public string? ImportedProfessionalName { get; set; }

    public DateTime ScrapedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
