using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.ProviderLeads;

public class IndexModel : PageModel
{
    private readonly IProviderLeadService _providerLeadService;
    private readonly ILookupService _lookupService;

    public IndexModel(IProviderLeadService providerLeadService, ILookupService lookupService)
    {
        _providerLeadService = providerLeadService;
        _lookupService = lookupService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeadSourceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ProfessionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? RegionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeadCaptureRunId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SiteKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ImportStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    public PagedResult<ProviderLeadListItemDto> Result { get; private set; } = new();

    public IReadOnlyList<SelectListItem> SourceOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> ProfessionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> RegionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> ImportStatusOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadFilterOptionsAsync(cancellationToken);
        await LoadResultsAsync(cancellationToken);
    }

    public async Task<PartialViewResult> OnGetResultsAsync(CancellationToken cancellationToken)
    {
        await LoadResultsAsync(cancellationToken);

        return new PartialViewResult
        {
            ViewName = "_Results",
            ViewData = new ViewDataDictionary<IndexModel>(ViewData, this)
        };
    }

    private async Task LoadFilterOptionsAsync(CancellationToken cancellationToken)
    {
        var sources = await _lookupService.GetSourcesAsync(cancellationToken: cancellationToken);
        var professions = await _lookupService.GetProfessionsAsync(cancellationToken: cancellationToken);
        var regions = await _lookupService.GetRegionsAsync(includeInactive: true, cancellationToken: cancellationToken);

        SourceOptions = sources.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        ProfessionOptions = professions.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
        RegionOptions = regions.OrderBy(x => x.DisplayName).Select(x => new SelectListItem(x.DisplayName, x.Id.ToString())).ToList();

        ImportStatusOptions =
        [
            new SelectListItem("Captured", "Captured"),
            new SelectListItem("Imported", "Imported"),
            new SelectListItem("Ignored", "Ignored"),
            new SelectListItem("Errored", "Errored")
        ];
    }

    private async Task LoadResultsAsync(CancellationToken cancellationToken)
    {
        Result = await _providerLeadService.GetPagedAsync(new ProviderLeadQueryFilter
        {
            SearchTerm = SearchTerm,
            LeadSourceId = LeadSourceId,
            ProfessionId = ProfessionId,
            RegionId = RegionId,
            LeadCaptureRunId = LeadCaptureRunId,
            SiteKey = SiteKey,
            City = City,
            ImportStatus = ImportStatus,
            PageNumber = PageNumber,
            PageSize = PageSize
        }, cancellationToken);
    }
}
