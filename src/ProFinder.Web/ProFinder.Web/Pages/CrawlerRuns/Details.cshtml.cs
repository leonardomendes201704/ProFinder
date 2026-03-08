using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.CrawlerRuns;

public class DetailsModel : PageModel
{
    private readonly ICrawlerRunService _crawlerRunService;

    public DetailsModel(ICrawlerRunService crawlerRunService)
    {
        _crawlerRunService = crawlerRunService;
    }

    public CrawlerRunDetailsDto Run { get; private set; } = new();

    public IReadOnlyList<CrawlerRunLogDto> Logs { get; private set; } = [];

    public bool CanStop =>
        Run.CanStop && Run.Status is "Queued" or "Starting" or "Running";

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await LoadAsync(id, cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/CrawlerRuns/Index");
        }
    }

    public async Task<IActionResult> OnPostStopAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _crawlerRunService.RequestStopAsync(id, "AdminUI", cancellationToken);
            TempData["SuccessMessage"] = "Solicitacao de parada enviada para o crawler.";
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToPage(new { id });
    }

    private async Task LoadAsync(int id, CancellationToken cancellationToken)
    {
        Run = await _crawlerRunService.GetByIdAsync(id, cancellationToken);
        Logs = await _crawlerRunService.GetLogsAsync(id, 300, cancellationToken);
    }
}
