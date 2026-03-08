using ProFinder.Domain.Common;

namespace ProFinder.Domain.Entities;

public class AppSetting : BaseEntity, IAuditableEntity
{
    public string Key { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string DataType { get; set; } = "string";

    public string Value { get; set; } = string.Empty;

    public string DefaultValue { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }

    public bool IsEditable { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
