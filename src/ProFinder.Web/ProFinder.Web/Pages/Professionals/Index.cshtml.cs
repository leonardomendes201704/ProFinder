using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professionals;

public class IndexModel : PageModel
{
    private readonly IProfessionalService _professionalService;
    private readonly ILookupService _lookupService;

    public IndexModel(IProfessionalService professionalService, ILookupService lookupService)
    {
        _professionalService = professionalService;
        _lookupService = lookupService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ProfessionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? StatusId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SourceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Neighborhood { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<ProfessionalListItemDto> Result { get; private set; } = new();

    public IReadOnlyList<SelectListItem> ProfessionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> StatusOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> SourceOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> CityOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> NeighborhoodOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFilterOptionsAsync(cancellationToken);

        var filter = new ProfessionalQueryFilter
        {
            SearchTerm = SearchTerm,
            ProfessionId = ProfessionId,
            StatusId = StatusId,
            SourceId = SourceId,
            City = City,
            Neighborhood = Neighborhood,
            PageNumber = PageNumber,
            PageSize = PageSize
        };

        Result = await _professionalService.GetPagedAsync(filter, cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _professionalService.DeleteAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Profissional desativado com sucesso.";
        return RedirectToPage();
    }

    private async Task LoadFilterOptionsAsync(CancellationToken cancellationToken)
    {
        var professions = await _lookupService.GetProfessionsAsync(cancellationToken: cancellationToken);
        var statuses = await _lookupService.GetStatusesAsync(cancellationToken: cancellationToken);
        var sources = await _lookupService.GetSourcesAsync(cancellationToken: cancellationToken);
        var regions = await _lookupService.GetRegionsAsync(cancellationToken: cancellationToken);

        ProfessionOptions = professions
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        StatusOptions = statuses
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        SourceOptions = sources
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        CityOptions = regions
            .Select(x => x.City)
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x))
            .ToList();

        NeighborhoodOptions = regions
            .Where(x => !string.IsNullOrWhiteSpace(x.Neighborhood))
            .Select(x => x.Neighborhood!)
            .Distinct()
            .OrderBy(x => x)
            .Select(x => new SelectListItem(x, x))
            .ToList();
    }
}
