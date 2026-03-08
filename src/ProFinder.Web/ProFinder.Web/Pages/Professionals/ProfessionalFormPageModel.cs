using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professionals;

public abstract class ProfessionalFormPageModel : PageModel
{
    private readonly ILookupService _lookupService;

    protected ProfessionalFormPageModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [BindProperty]
    public UpsertProfessionalDto Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ProfessionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> SourceOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> StatusOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> RegionOptions { get; private set; } = [];

    protected async Task LoadOptionsAsync(CancellationToken cancellationToken)
    {
        var professions = await _lookupService.GetProfessionsAsync(cancellationToken: cancellationToken);
        var sources = await _lookupService.GetSourcesAsync(cancellationToken: cancellationToken);
        var statuses = await _lookupService.GetStatusesAsync(cancellationToken: cancellationToken);
        var regions = await _lookupService.GetRegionsAsync(cancellationToken: cancellationToken);

        ProfessionOptions = professions
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        SourceOptions = sources
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        StatusOptions = statuses
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        RegionOptions = regions
            .OrderBy(x => x.DisplayName)
            .Select(x => new SelectListItem(x.DisplayName, x.Id.ToString()))
            .ToList();
    }
}
