using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Regions;

public class LookupModel : PageModel
{
    private readonly IGeographicReferenceService _geographicReferenceService;

    public LookupModel(IGeographicReferenceService geographicReferenceService)
    {
        _geographicReferenceService = geographicReferenceService;
    }

    public async Task<JsonResult> OnGetCitiesAsync(string stateCode, CancellationToken cancellationToken)
    {
        var cities = await _geographicReferenceService.GetCitiesByStateAsync(stateCode, cancellationToken);
        return new JsonResult(cities);
    }

    public async Task<JsonResult> OnGetDistrictsAsync(int cityId, CancellationToken cancellationToken)
    {
        var districts = await _geographicReferenceService.GetDistrictsByCityAsync(cityId, cancellationToken);
        return new JsonResult(districts);
    }

    public async Task<JsonResult> OnGetZipCodeAsync(string zipCode, CancellationToken cancellationToken)
    {
        var result = await _geographicReferenceService.LookupZipCodeAsync(zipCode, cancellationToken);
        return new JsonResult(result);
    }
}
