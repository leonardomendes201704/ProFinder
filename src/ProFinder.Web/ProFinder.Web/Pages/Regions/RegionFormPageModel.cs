using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProFinder.Application.DTOs.Geography;
using ProFinder.Application.DTOs.Regions;
using ProFinder.Application.Interfaces.Services;

namespace ProFinder.Web.Pages.Regions;

public abstract class RegionFormPageModel : PageModel
{
    private readonly IGeographicReferenceService _geographicReferenceService;

    protected RegionFormPageModel(IGeographicReferenceService geographicReferenceService)
    {
        _geographicReferenceService = geographicReferenceService;
    }

    [BindProperty]
    public UpsertRegionDto Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> StateOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> CityOptions { get; private set; } = [];

    public IReadOnlyList<SelectListItem> NeighborhoodOptions { get; private set; } = [];

    protected async Task LoadReferenceDataAsync(CancellationToken cancellationToken)
    {
        var states = await _geographicReferenceService.GetStatesAsync(cancellationToken);
        StateOptions = states
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem($"{x.Name} ({x.Code})", x.Code, string.Equals(x.Code, Input.State, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var cities = await LoadCityOptionsAsync(Input.State, Input.City, cancellationToken);
        var selectedCity = cities.FirstOrDefault(x => string.Equals(x.Name, Input.City, StringComparison.OrdinalIgnoreCase));
        await LoadNeighborhoodOptionsAsync(selectedCity?.Id, Input.Neighborhood, cancellationToken);
    }

    private async Task<IReadOnlyList<CityOptionDto>> LoadCityOptionsAsync(
        string? stateCode,
        string? selectedCityName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
        {
            CityOptions = [];
            return [];
        }

        var cities = (await _geographicReferenceService.GetCitiesByStateAsync(stateCode, cancellationToken)).ToList();
        EnsureCityOptionExists(cities, selectedCityName);

        CityOptions = cities
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Name, string.Equals(x.Name, selectedCityName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return cities;
    }

    private async Task LoadNeighborhoodOptionsAsync(int? cityId, string? selectedNeighborhood, CancellationToken cancellationToken)
    {
        if (!cityId.HasValue || cityId.Value <= 0)
        {
            NeighborhoodOptions = BuildDistrictOptions([], selectedNeighborhood);
            return;
        }

        var districts = (await _geographicReferenceService.GetDistrictsByCityAsync(cityId.Value, cancellationToken)).ToList();
        EnsureDistrictOptionExists(districts, selectedNeighborhood);
        NeighborhoodOptions = BuildDistrictOptions(districts, selectedNeighborhood);
    }

    private static IReadOnlyList<SelectListItem> BuildDistrictOptions(
        IReadOnlyCollection<DistrictOptionDto> districts,
        string? selectedNeighborhood) =>
        districts
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Name, string.Equals(x.Name, selectedNeighborhood, StringComparison.OrdinalIgnoreCase)))
            .ToList();

    private static void EnsureCityOptionExists(ICollection<CityOptionDto> cities, string? selectedCityName)
    {
        if (string.IsNullOrWhiteSpace(selectedCityName))
        {
            return;
        }

        if (cities.Any(x => string.Equals(x.Name, selectedCityName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        cities.Add(new CityOptionDto
        {
            Id = 0,
            Name = selectedCityName.Trim()
        });
    }

    private static void EnsureDistrictOptionExists(ICollection<DistrictOptionDto> districts, string? selectedNeighborhood)
    {
        if (string.IsNullOrWhiteSpace(selectedNeighborhood))
        {
            return;
        }

        if (districts.Any(x => string.Equals(x.Name, selectedNeighborhood, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        districts.Add(new DistrictOptionDto
        {
            Id = 0,
            Name = selectedNeighborhood.Trim()
        });
    }
}
