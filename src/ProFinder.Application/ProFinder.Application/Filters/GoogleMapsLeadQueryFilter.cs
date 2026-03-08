namespace ProFinder.Application.Filters;

public class GoogleMapsLeadQueryFilter
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public string? SearchTerm { get; set; }

    public int? ProfessionId { get; set; }

    public int? RegionId { get; set; }

    public int? LeadCaptureRunId { get; set; }

    public string? ImportStatus { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is <= 0 or > 100 ? 20 : value;
    }
}
