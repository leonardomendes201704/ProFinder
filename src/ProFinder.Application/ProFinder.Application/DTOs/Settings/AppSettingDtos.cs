namespace ProFinder.Application.DTOs.Settings;

public class AppSettingDto
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string DataType { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string DefaultValue { get; set; } = string.Empty;

    public bool IsSensitive { get; set; }

    public bool IsEditable { get; set; }

    public int DisplayOrder { get; set; }
}

public class AppSettingGroupDto
{
    public string Category { get; set; } = string.Empty;

    public IReadOnlyList<AppSettingDto> Items { get; set; } = [];
}

public class UpdateAppSettingDto
{
    public string Value { get; set; } = string.Empty;
}
