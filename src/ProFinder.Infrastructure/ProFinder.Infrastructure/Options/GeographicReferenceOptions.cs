namespace ProFinder.Infrastructure.Options;

public class GeographicReferenceOptions
{
    public const string SectionName = "ExternalServices";

    public string IbgeBaseUrl { get; set; } = "https://servicodados.ibge.gov.br";

    public string ViaCepBaseUrl { get; set; } = "https://viacep.com.br";

    public string NominatimBaseUrl { get; set; } = "https://nominatim.openstreetmap.org";

    public string NominatimUserAgent { get; set; } = "ProFinder/1.0 (+https://localhost)";
}
