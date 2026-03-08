using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Statuses;

public class IndexModel : PageModel
{
    private readonly ILookupService _lookupService;

    public IndexModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    public IReadOnlyList<ProFinder.Application.DTOs.Statuses.LeadStatusDto> Items { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Items = await _lookupService.GetStatusesAsync(includeInactive: true, cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteStatusAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Status desativado com sucesso.";
        return RedirectToPage();
    }
}
