using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class ProfessionalRegion : BaseEntity
{
    public int ProfessionalId { get; set; }

    public int RegionId { get; set; }

    public string? ConfidenceLevel { get; set; }

    public bool IsPrimaryRegion { get; set; }

    public Professional? Professional { get; set; }

    public Region? Region { get; set; }
}
