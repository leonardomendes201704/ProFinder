namespace ProFinder.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProfessionals { get; set; }

    public IReadOnlyList<DashboardMetricItemDto> ProfessionalsByProfession { get; set; } = [];

    public IReadOnlyList<DashboardMetricItemDto> ProfessionalsByStatus { get; set; } = [];

    public IReadOnlyList<DashboardMetricItemDto> ProfessionalsByCity { get; set; } = [];

    public IReadOnlyList<RecentProfessionalDto> RecentProfessionals { get; set; } = [];
}

public class DashboardMetricItemDto
{
    public string Label { get; set; } = string.Empty;

    public int Total { get; set; }
}

public class RecentProfessionalDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string ProfessionName { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? City { get; set; }

    public DateTime CreatedAt { get; set; }
}
