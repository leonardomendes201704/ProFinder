using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using ProFinder.Application.DTOs.Geography;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Infrastructure.Options;

namespace ProFinder.Infrastructure.Services;

public class GeographicReferenceService : IGeographicReferenceService
{
    private const string BrazilCountryName = "Brasil";
    private readonly ILogger<GeographicReferenceService> _logger;
    private readonly GeographicReferenceOptions _options;

    public GeographicReferenceService(GeographicReferenceOptions options, ILogger<GeographicReferenceService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StateOptionDto>> GetStatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient(_options.IbgeBaseUrl);
            var response = await client.GetFromJsonAsync<List<IbgeStateResponse>>(
                "/api/v1/localidades/estados?orderBy=nome",
                cancellationToken);

            return response?
                .OrderBy(x => x.Name)
                .Select(x => new StateOptionDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
                .ToList() ?? [];
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not load states from IBGE.");
            return [];
        }
    }

    public async Task<IReadOnlyList<CityOptionDto>> GetCitiesByStateAsync(string stateCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(stateCode))
        {
            return [];
        }

        try
        {
            using var client = CreateClient(_options.IbgeBaseUrl);
            var normalizedStateCode = stateCode.Trim().ToUpperInvariant();
            var response = await client.GetFromJsonAsync<List<IbgeCityResponse>>(
                $"/api/v1/localidades/estados/{normalizedStateCode}/municipios?orderBy=nome",
                cancellationToken);

            return response?
                .OrderBy(x => x.Name)
                .Select(x => new CityOptionDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList() ?? [];
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not load cities from IBGE for state {StateCode}.", stateCode);
            return [];
        }
    }

    public async Task<IReadOnlyList<DistrictOptionDto>> GetDistrictsByCityAsync(int cityId, CancellationToken cancellationToken = default)
    {
        if (cityId <= 0)
        {
            return [];
        }

        try
        {
            using var client = CreateClient(_options.IbgeBaseUrl);
            var response = await client.GetFromJsonAsync<List<IbgeDistrictResponse>>(
                $"/api/v1/localidades/municipios/{cityId}/distritos?orderBy=nome",
                cancellationToken);

            return response?
                .OrderBy(x => x.Name)
                .Select(x => new DistrictOptionDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList() ?? [];
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not load districts from IBGE for city {CityId}.", cityId);
            return [];
        }
    }

    public async Task<ZipCodeLookupResultDto> LookupZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var normalizedZipCode = NormalizeZipCode(zipCode);
        if (string.IsNullOrWhiteSpace(normalizedZipCode))
        {
            return new ZipCodeLookupResultDto();
        }

        try
        {
            using var viaCepClient = CreateClient(_options.ViaCepBaseUrl);
            var address = await viaCepClient.GetFromJsonAsync<ViaCepResponse>(
                $"/ws/{normalizedZipCode}/json/",
                cancellationToken);

            if (address is null || address.HasError || string.IsNullOrWhiteSpace(address.StateCode) || string.IsNullOrWhiteSpace(address.CityName))
            {
                return new ZipCodeLookupResultDto();
            }

            var result = new ZipCodeLookupResultDto
            {
                ZipCode = FormatZipCode(normalizedZipCode),
                StateCode = address.StateCode.Trim().ToUpperInvariant(),
                StateName = address.StateName?.Trim() ?? string.Empty,
                CityName = address.CityName.Trim(),
                CityId = ParseIntOrNull(address.CityIbgeCode),
                DistrictName = string.IsNullOrWhiteSpace(address.DistrictName) ? null : address.DistrictName.Trim()
            };

            var coordinates = await ResolveCoordinatesAsync(result, cancellationToken);
            result.Latitude = coordinates.Latitude;
            result.Longitude = coordinates.Longitude;

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not resolve location information for ZIP code {ZipCode}.", normalizedZipCode);
            return new ZipCodeLookupResultDto();
        }
    }

    private async Task<(decimal? Latitude, decimal? Longitude)> ResolveCoordinatesAsync(
        ZipCodeLookupResultDto lookupResult,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = CreateNominatimClient();
            var queries = BuildGeocodingQueries(lookupResult).ToList();

            foreach (var query in queries)
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var response = await client.GetFromJsonAsync<List<NominatimSearchResponse>>(
                    $"/search?format=jsonv2&limit=1&q={encodedQuery}",
                    cancellationToken);

                var item = response?.FirstOrDefault();
                if (item is null)
                {
                    continue;
                }

                if (decimal.TryParse(item.Latitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) &&
                    decimal.TryParse(item.Longitude, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                {
                    return (latitude, longitude);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not resolve coordinates for ZIP code {ZipCode}.", lookupResult.ZipCode);
        }

        return (null, null);
    }

    private static HttpClient CreateClient(string baseUrl) =>
        new()
        {
            BaseAddress = new Uri(baseUrl)
        };

    private HttpClient CreateNominatimClient()
    {
        var client = CreateClient(_options.NominatimBaseUrl);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.NominatimUserAgent);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static IEnumerable<string> BuildGeocodingQueries(ZipCodeLookupResultDto lookupResult)
    {
        var zipCode = NormalizeZipCode(lookupResult.ZipCode);
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(zipCode))
        {
            parts.Add(zipCode);
        }

        if (!string.IsNullOrWhiteSpace(lookupResult.DistrictName))
        {
            parts.Add(lookupResult.DistrictName);
        }

        if (!string.IsNullOrWhiteSpace(lookupResult.CityName))
        {
            parts.Add(lookupResult.CityName);
        }

        if (!string.IsNullOrWhiteSpace(lookupResult.StateCode))
        {
            parts.Add(lookupResult.StateCode);
        }

        parts.Add(BrazilCountryName);

        yield return string.Join(", ", parts);

        if (!string.IsNullOrWhiteSpace(zipCode))
        {
            yield return $"{zipCode}, {lookupResult.CityName}, {lookupResult.StateCode}, {BrazilCountryName}";
        }
    }

    private static string NormalizeZipCode(string? zipCode) =>
        string.IsNullOrWhiteSpace(zipCode)
            ? string.Empty
            : new string(zipCode.Where(char.IsDigit).ToArray());

    private static string FormatZipCode(string zipCode) =>
        zipCode.Length == 8
            ? $"{zipCode[..5]}-{zipCode[5..]}"
            : zipCode;

    private static int? ParseIntOrNull(string? value) =>
        int.TryParse(value, out var number) ? number : null;

    private sealed class IbgeStateResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sigla")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class IbgeCityResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class IbgeDistrictResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Name { get; set; } = string.Empty;
    }

    private sealed class ViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string? ZipCode { get; set; }

        [JsonPropertyName("uf")]
        public string? StateCode { get; set; }

        [JsonPropertyName("estado")]
        public string? StateName { get; set; }

        [JsonPropertyName("localidade")]
        public string? CityName { get; set; }

        [JsonPropertyName("bairro")]
        public string? DistrictName { get; set; }

        [JsonPropertyName("ibge")]
        public string? CityIbgeCode { get; set; }

        [JsonPropertyName("erro")]
        public bool HasError { get; set; }
    }

    private sealed class NominatimSearchResponse
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; set; } = string.Empty;

        [JsonPropertyName("lon")]
        public string Longitude { get; set; } = string.Empty;
    }
}
