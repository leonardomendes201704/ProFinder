using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class Profession : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Professional> Professionals { get; set; } = [];

    public ICollection<ProviderLead> ProviderLeads { get; set; } = [];

    public ICollection<ProfessionalProfession> ProfessionalProfessions { get; set; } = [];

    public ICollection<ProviderLeadProfession> ProviderLeadProfessions { get; set; } = [];
}
