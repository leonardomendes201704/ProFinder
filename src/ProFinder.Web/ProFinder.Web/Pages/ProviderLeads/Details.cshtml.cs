using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.ProviderLeads;

public class DetailsModel : PageModel
{
    private readonly IProviderLeadService _providerLeadService;

    public DetailsModel(IProviderLeadService providerLeadService)
    {
        _providerLeadService = providerLeadService;
    }

    public ProviderLeadDetailsDto Item { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            Item = await _providerLeadService.GetByIdAsync(id, cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/ProviderLeads/Index");
        }
    }
}
