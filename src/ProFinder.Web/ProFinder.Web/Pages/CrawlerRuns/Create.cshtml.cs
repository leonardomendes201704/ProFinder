using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.CrawlerRuns;

public class CreateModel : PageModel
{
    private readonly ICrawlerRunService _crawlerRunService;
    private readonly ILookupService _lookupService;

    public CreateModel(ICrawlerRunService crawlerRunService, ILookupService lookupService)
    {
        _crawlerRunService = crawlerRunService;
        _lookupService = lookupService;
    }

    [BindProperty]
    public StartCrawlerRunRequestDto Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> ProfessionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> RegionOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> HeadlessOptions { get; private set; } = [];

    public IReadOnlyList<SiteOption> SiteOptions { get; private set; } = [];

    public CrawlerLauncherDefaultsDto LauncherDefaults { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
        Input.RequestedBy = "AdminUI";
        Input.SelectedSites = GetDefaultSites();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);

        if (Input.SelectedSites.Count == 0)
        {
            ModelState.AddModelError("Input.SelectedSites", "Selecione pelo menos um site.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var run = await _crawlerRunService.QueueAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = $"Lote #{run.Id} enviado para execucao.";
            return RedirectToPage("/CrawlerRuns/Details", new { id = run.Id });
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var professions = await _lookupService.GetProfessionsAsync(cancellationToken: cancellationToken);
        var regions = await _lookupService.GetRegionsAsync(includeInactive: true, cancellationToken: cancellationToken);
        LauncherDefaults = await _crawlerRunService.GetLauncherDefaultsAsync(cancellationToken);

        ProfessionOptions = professions
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();

        RegionOptions = regions
            .OrderBy(x => x.DisplayName)
            .Select(x => new SelectListItem(x.DisplayName, x.Id.ToString()))
            .ToList();

        HeadlessOptions =
        [
            new SelectListItem("Usar configuracao do sistema", string.Empty),
            new SelectListItem("Forcar headless", "true"),
            new SelectListItem("Abrir navegador visivel", "false")
        ];

        SiteOptions =
        [
            new SiteOption("google_maps", "Google Maps", "Busca dinamica via Selenium."),
            new SiteOption("olx", "OLX", "HTTP first com fallback em browser."),
            new SiteOption("telelistas", "Telelistas", "HTTP first com fallback em browser."),
            new SiteOption("guiamais", "GuiaMais", "HTTP first com fallback em browser.")
        ];
    }

    private List<string> GetDefaultSites() =>
        LauncherDefaults.DefaultSitesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .ToList();

    public sealed record SiteOption(string Value, string Label, string Description);
}
