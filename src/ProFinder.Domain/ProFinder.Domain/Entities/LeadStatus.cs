using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class LeadStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Professional> Professionals { get; set; } = [];
}
