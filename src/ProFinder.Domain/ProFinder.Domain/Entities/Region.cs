using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class Region : BaseEntity
{
    public string State { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Neighborhood { get; set; }

    public string? ZipCode { get; set; }

    public string? Zone { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? RadiusKm { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProfessionalRegion> ProfessionalRegions { get; set; } = [];

    public ICollection<ProviderLead> ProviderLeads { get; set; } = [];
}
