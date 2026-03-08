namespace ProFinder.Application.Filters;

public class ProviderLeadQueryFilter
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public string? SearchTerm { get; set; }

    public int? LeadSourceId { get; set; }

    public int? ProfessionId { get; set; }

    public int? RegionId { get; set; }

    public int? LeadCaptureRunId { get; set; }

    public string? SiteKey { get; set; }

    public string? City { get; set; }

    public string? ImportStatus { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }
}
