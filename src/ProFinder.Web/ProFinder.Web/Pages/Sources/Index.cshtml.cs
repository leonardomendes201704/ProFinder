using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Sources;

public class IndexModel : PageModel
{
    private readonly ILookupService _lookupService;

    public IndexModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    public IReadOnlyList<ProFinder.Application.DTOs.Sources.LeadSourceDto> Items { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Items = await _lookupService.GetSourcesAsync(includeInactive: true, cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteSourceAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Origem desativada com sucesso.";
        return RedirectToPage();
    }
}
