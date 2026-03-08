using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class ProviderLeadProfession : BaseEntity
{
    public int ProviderLeadId { get; set; }

    public int ProfessionId { get; set; }

    public bool IsPrimary { get; set; }

    public ProviderLead? ProviderLead { get; set; }

    public Profession? Profession { get; set; }
}
