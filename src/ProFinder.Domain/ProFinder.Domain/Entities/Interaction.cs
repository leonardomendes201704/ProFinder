using ProFinder.Domain.Common;
using ProFinder.Domain.Enums;

namespace ProFinder.Domain.Entities;

public class Interaction : BaseEntity
{
    public int ProfessionalId { get; set; }

    public InteractionType InteractionType { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime InteractionDate { get; set; }

    public string? CreatedBy { get; set; }

    public Professional? Professional { get; set; }
}
