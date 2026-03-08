using ProFinder.Application.DTOs.Geography;

namespace ProFinder.Application.Interfaces.Services;

public interface IGeographicReferenceService
{
    Task<IReadOnlyList<StateOptionDto>> GetStatesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CityOptionDto>> GetCitiesByStateAsync(string stateCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DistrictOptionDto>> GetDistrictsByCityAsync(int cityId, CancellationToken cancellationToken = default);

    Task<ZipCodeLookupResultDto> LookupZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
}
