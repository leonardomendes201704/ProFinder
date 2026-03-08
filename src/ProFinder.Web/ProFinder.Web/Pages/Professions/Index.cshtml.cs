using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Professions;

public class IndexModel : PageModel
{
    private readonly ILookupService _lookupService;

    public IndexModel(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    public IReadOnlyList<ProFinder.Application.DTOs.Professions.ProfessionDto> Items { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Items = await _lookupService.GetProfessionsAsync(includeInactive: true, cancellationToken);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _lookupService.DeleteProfessionAsync(id, cancellationToken);
        TempData["SuccessMessage"] = "Profissão desativada com sucesso.";
        return RedirectToPage();
    }
}
