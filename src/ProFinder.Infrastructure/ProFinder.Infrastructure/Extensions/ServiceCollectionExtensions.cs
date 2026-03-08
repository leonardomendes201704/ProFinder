using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Options;
using ProFinder.Infrastructure.Services;

namespace ProFinder.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var geographicSection = configuration.GetSection(GeographicReferenceOptions.SectionName);
        services.AddSingleton(new GeographicReferenceOptions
        {
            IbgeBaseUrl = geographicSection["IbgeBaseUrl"] ?? "https://servicodados.ibge.gov.br",
            ViaCepBaseUrl = geographicSection["ViaCepBaseUrl"] ?? "https://viacep.com.br",
            NominatimBaseUrl = geographicSection["NominatimBaseUrl"] ?? "https://nominatim.openstreetmap.org",
            NominatimUserAgent = geographicSection["NominatimUserAgent"] ?? "ProFinder/1.0 (+https://localhost)"
        });

        services.AddDbContext<ProFinderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(ProFinderDbContext).Assembly.FullName)));

        services.AddSingleton<CrawlerProcessRegistry>();
        services.AddSingleton<ICrawlerRunRealtimeNotifier, NoOpCrawlerRunRealtimeNotifier>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAppSettingService, AppSettingService>();
        services.AddScoped<ICrawlerRunService, CrawlerRunService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IProfessionalService, ProfessionalService>();
        services.AddScoped<IGoogleMapsLeadService, GoogleMapsLeadService>();
        services.AddScoped<IProviderLeadService, ProviderLeadService>();
        services.AddScoped<IInteractionService, InteractionService>();
        services.AddScoped<IGeographicReferenceService, GeographicReferenceService>();
        services.AddScoped<CsvLeadImporterService>();
        services.AddScoped<ILeadCaptureService, LeadCaptureService>();

        return services;
    }
}
