using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Regions;

public class EditModel : RegionFormPageModel
{
    private readonly ILookupService _lookupService;

    public EditModel(ILookupService lookupService, IGeographicReferenceService geographicReferenceService)
        : base(geographicReferenceService)
    {
        _lookupService = lookupService;
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _lookupService.GetRegionByIdAsync(id, cancellationToken);
            Input = new()
            {
                State = item.State,
                City = item.City,
                Neighborhood = item.Neighborhood,
                ZipCode = item.ZipCode,
                Zone = item.Zone,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                RadiusKm = item.RadiusKm,
                IsActive = item.IsActive
            };

            await LoadReferenceDataAsync(cancellationToken);
            return Page();
        }
        catch (AppException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return RedirectToPage("/Regions/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadReferenceDataAsync(cancellationToken);
            return Page();
        }

        try
        {
            await _lookupService.UpdateRegionAsync(id, Input, cancellationToken);
            TempData["SuccessMessage"] = "Região atualizada com sucesso.";
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
