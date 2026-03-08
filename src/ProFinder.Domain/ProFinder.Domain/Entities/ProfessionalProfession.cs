using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class ProfessionalProfession : BaseEntity
{
    public int ProfessionalId { get; set; }

    public int ProfessionId { get; set; }

    public bool IsPrimary { get; set; }

    public Professional? Professional { get; set; }

    public Profession? Profession { get; set; }
}
