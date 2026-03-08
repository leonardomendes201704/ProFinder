using Microsoft.EntityFrameworkCore;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Settings;

public static class CrawlerRuntimeSettingCatalog
{
    private sealed record SettingDefinition(
        string Key,
        string Category,
        string DisplayName,
        string Description,
        string DataType,
        string DefaultValue,
        bool IsSensitive,
        int DisplayOrder);

    private static readonly SettingDefinition[] Definitions =
    [
        new(
            "crawler.concurrent_workers",
            "Crawler",
            "Workers concorrentes",
            "Quantidade maxima de workers assincronos para o scheduler.",
            "int",
            "4",
            false,
            1),
        new(
            "crawler.http_concurrency",
            "Crawler",
            "Concorrencia HTTP",
            "Limite de requisicoes HTTP simultaneas.",
            "int",
            "12",
            false,
            2),
        new(
            "crawler.http_timeout_seconds",
            "Crawler",
            "Timeout HTTP (segundos)",
            "Timeout padrao das requisicoes HTTP.",
            "int",
            "30",
            false,
            3),
        new(
            "crawler.max_retries",
            "Crawler",
            "Maximo de tentativas",
            "Numero maximo de retries por requisicao.",
            "int",
            "3",
            false,
            4),
        new(
            "crawler.request_delay_ms",
            "Crawler",
            "Delay entre requisicoes (ms)",
            "Intervalo minimo entre chamadas para a mesma fonte.",
            "int",
            "700",
            false,
            5),
        new(
            "crawler.browser_fallback_enabled",
            "Crawler",
            "Fallback de navegador",
            "Permite trocar de HTTP para browser automation quando a fonte for dinamica.",
            "bool",
            "true",
            false,
            6),
        new(
            "crawler.user_agent_rotation_enabled",
            "Crawler",
            "Rotacao de user-agent",
            "Habilita rotacao simples de user-agents.",
            "bool",
            "true",
            false,
            7),
        new(
            "crawler.proxy_list_json",
            "Crawler",
            "Lista de proxies",
            "JSON com proxies opcionais para rotacao.",
            "json",
            "[]",
            false,
            8),
        new(
            "crawler.max_pages_per_target",
            "Crawler",
            "Maximo de paginas por alvo",
            "Limite de paginas a percorrer por site e alvo.",
            "int",
            "50",
            false,
            9),
        new(
            "crawler.max_records_per_run",
            "Crawler",
            "Maximo de leads por execucao",
            "Quantidade maxima de registros persistidos por execucao.",
            "int",
            "5000",
            false,
            10),
        new(
            "crawler.browser_headless",
            "Crawler",
            "Executar navegadores em headless",
            "Controla se Selenium e Playwright rodam em modo headless.",
            "bool",
            "true",
            false,
            11),
        new(
            "crawler.browser.chrome_arguments_json",
            "Crawler",
            "Argumentos extras do Chrome",
            "JSON com argumentos extras usados ao iniciar o Chrome no Selenium. Em Linux container, mantenha --no-sandbox e --disable-dev-shm-usage.",
            "json",
            "[\"--no-sandbox\",\"--disable-dev-shm-usage\",\"--disable-gpu\",\"--disable-software-rasterizer\",\"--remote-debugging-pipe\"]",
            false,
            12),
        new(
            "crawler.google_maps.max_idle_scrolls",
            "GoogleMaps",
            "Scrolls ociosos maximos",
            "Limite de scrolls sem novos cards no Google Maps.",
            "int",
            "8",
            false,
            1),
        new(
            "crawler.google_maps.scroll_pause_ms",
            "GoogleMaps",
            "Pausa do scroll (ms)",
            "Pausa entre scrolls da lista do Google Maps.",
            "int",
            "1500",
            false,
            2)
    ];

    public static async Task EnsureDefaultsAsync(ProFinderDbContext context, CancellationToken cancellationToken = default)
    {
        var keys = Definitions.Select(x => x.Key).ToArray();
        var existingKeys = await context.AppSettings
            .Where(x => keys.Contains(x.Key))
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        if (existingKeys.Count == Definitions.Length)
        {
            return;
        }

        var utcNow = DateTime.UtcNow;
        foreach (var definition in Definitions.Where(x => !existingKeys.Contains(x.Key)))
        {
            context.AppSettings.Add(new AppSetting
            {
                Key = definition.Key,
                Category = definition.Category,
                DisplayName = definition.DisplayName,
                Description = definition.Description,
                DataType = definition.DataType,
                Value = definition.DefaultValue,
                DefaultValue = definition.DefaultValue,
                IsSensitive = definition.IsSensitive,
                IsEditable = true,
                DisplayOrder = definition.DisplayOrder,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
