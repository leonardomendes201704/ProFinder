using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class Professional : BaseEntity, IAuditableEntity
{
    public string FullName { get; set; } = string.Empty;

    public string? BusinessName { get; set; }

    public string? Phone { get; set; }

    public string? WhatsApp { get; set; }

    public string? Email { get; set; }

    public string? DocumentNumber { get; set; }

    public int ProfessionId { get; set; }

    public int SourceId { get; set; }

    public int StatusId { get; set; }

    public string? Notes { get; set; }

    public string? Website { get; set; }

    public string? Instagram { get; set; }

    public bool IsAutonomous { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Profession? Profession { get; set; }

    public LeadSource? Source { get; set; }

    public LeadStatus? Status { get; set; }

    public ICollection<ProfessionalRegion> ProfessionalRegions { get; set; } = [];

    public ICollection<ProfessionalProfession> ProfessionalProfessions { get; set; } = [];

    public ICollection<Interaction> Interactions { get; set; } = [];

    public ICollection<Evidence> Evidences { get; set; } = [];

    public ICollection<ProviderLead> ImportedProviderLeads { get; set; } = [];
}
