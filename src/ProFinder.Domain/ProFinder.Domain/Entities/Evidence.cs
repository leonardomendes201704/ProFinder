using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class Evidence : BaseEntity
{
    public int ProfessionalId { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string FieldValue { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public DateTime CollectedAt { get; set; }

    public Professional? Professional { get; set; }
}
