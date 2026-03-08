using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Regions;

public class CreateModel : RegionFormPageModel
{
    private readonly ILookupService _lookupService;

    public CreateModel(ILookupService lookupService, IGeographicReferenceService geographicReferenceService)
        : base(geographicReferenceService)
    {
        _lookupService = lookupService;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Input.IsActive = true;
        await LoadReferenceDataAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadReferenceDataAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _lookupService.CreateRegionAsync(Input, cancellationToken);
            TempData["SuccessMessage"] = "Região cadastrada com sucesso.";
            return RedirectToPage("/Regions/Index");
        }
        catch (AppException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadReferenceDataAsync(cancellationToken);
            return Page();
        }
    }
}
