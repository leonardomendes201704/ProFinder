using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.CrawlerRuns;

public class IndexModel : PageModel
{
    private readonly ICrawlerRunService _crawlerRunService;

    public IndexModel(ICrawlerRunService crawlerRunService)
    {
        _crawlerRunService = crawlerRunService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    [BindProperty]
    public string? ResetConfirmation { get; set; }

    public PagedResult<CrawlerRunListItemDto> Result { get; private set; } = new();

    public IReadOnlyList<SelectListItem> StatusOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        StatusOptions =
        [
            new SelectListItem("Queued", "Queued"),
            new SelectListItem("Starting", "Starting"),
            new SelectListItem("Running", "Running"),
            new SelectListItem("Stopping", "Stopping"),
            new SelectListItem("Stopped", "Stopped"),
            new SelectListItem("Completed", "Completed"),
            new SelectListItem("Failed", "Failed")
        ];

        Result = await _crawlerRunService.GetPagedAsync(new CrawlerRunQueryFilter
        {
            SearchTerm = SearchTerm,
            Status = Status,
            PageNumber = PageNumber,
            PageSize = PageSize
        }, cancellationToken);
    }

    public async Task<IActionResult> OnPostResetAsync(CancellationToken cancellationToken)
    {
        if (!string.Equals(ResetConfirmation?.Trim(), "ZERAR", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Digite ZERAR para confirmar o reset dos dados do crawler.";
            return RedirectToPage("/CrawlerRuns/Index");
        }

        try
        {
            var result = await _crawlerRunService.ResetDataAsync("AdminUI", cancellationToken);
            TempData["SuccessMessage"] =
                $"Dados do crawler resetados. Lotes removidos: {result.LeadCaptureRunCount}. Leads consolidados: {result.ProviderLeadCount}. Google Maps: {result.GoogleMapsLeadCount}. Logs: {result.LeadCaptureLogCount}.";
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToPage("/CrawlerRuns/Index");
    }
}
