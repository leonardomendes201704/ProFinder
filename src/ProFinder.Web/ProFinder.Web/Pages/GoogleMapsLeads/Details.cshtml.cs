using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.GoogleMapsLeads;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.GoogleMapsLeads;

public class DetailsModel : PageModel
{
    private readonly IGoogleMapsLeadService _googleMapsLeadService;

    public DetailsModel(IGoogleMapsLeadService googleMapsLeadService)
    {
        _googleMapsLeadService = googleMapsLeadService;
    }

    public GoogleMapsLeadDetailsDto Lead { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            Lead = await _googleMapsLeadService.GetByIdAsync(id, cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/GoogleMapsLeads/Index");
        }
    }
}
