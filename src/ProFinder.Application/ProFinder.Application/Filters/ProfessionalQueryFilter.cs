namespace ProFinder.Application.Filters;

public class ProfessionalQueryFilter
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public string? SearchTerm { get; set; }

    public int? ProfessionId { get; set; }

    public int? StatusId { get; set; }

    public int? SourceId { get; set; }

    public string? City { get; set; }

    public string? Neighborhood { get; set; }

    public bool OnlyActive { get; set; } = true;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 10 : Math.Min(value, MaxPageSize);
    }
}
