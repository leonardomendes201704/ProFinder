using Microsoft.EntityFrameworkCore;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Settings;

public static class CrawlerLauncherSettingCatalog
{
    public const string Category = "CrawlerLauncher";
    public const string PythonExecutableKey = "crawler.launcher.python_executable";
    public const string ScriptPathKey = "crawler.launcher.script_path";
    public const string WorkingDirectoryKey = "crawler.launcher.working_directory";
    public const string ExportDirectoryKey = "crawler.launcher.export_directory";
    public const string SqlDriverKey = "crawler.launcher.sql_driver";
    public const string ConnectionStringOverrideKey = "crawler.launcher.connection_string_override";
    public const string DefaultSitesKey = "crawler.launcher.default_sites_csv";
    public const string LogLevelKey = "crawler.launcher.log_level";

    private sealed record SettingDefinition(
        string Key,
        string DisplayName,
        string Description,
        string DataType,
        string DefaultValue,
        bool IsSensitive,
        int DisplayOrder);

    private static readonly SettingDefinition[] Definitions =
    [
        new(
            PythonExecutableKey,
            "Executavel do Python",
            "Comando ou caminho do Python usado para iniciar o crawler pela UI.",
            "string",
            "python",
            false,
            1),
        new(
            ScriptPathKey,
            "Caminho do script do crawler",
            "Caminho absoluto ou relativo do arquivo crawler/main.py.",
            "string",
            "crawler/main.py",
            false,
            2),
        new(
            WorkingDirectoryKey,
            "Diretorio de trabalho do crawler",
            "Diretorio base para iniciar o processo. Em branco usa a raiz da solution quando encontrada.",
            "string",
            string.Empty,
            false,
            3),
        new(
            ExportDirectoryKey,
            "Diretorio de exportacao",
            "Diretorio onde o crawler grava providers.csv e providers.json.",
            "string",
            "crawler/exports",
            false,
            4),
        new(
            SqlDriverKey,
            "Driver ODBC do SQL Server",
            "Driver usado para converter a connection string da aplicacao em connection string ODBC para o Python.",
            "string",
            "ODBC Driver 18 for SQL Server",
            false,
            5),
        new(
            ConnectionStringOverrideKey,
            "Connection string ODBC override",
            "Quando preenchida, substitui a conversao automatica da connection string do .NET ao iniciar o crawler.",
            "string",
            string.Empty,
            true,
            6),
        new(
            DefaultSitesKey,
            "Sites padrao do launcher",
            "Lista CSV dos sites marcados por padrao no formulario do crawler.",
            "string",
            "google_maps,olx,telelistas,guiamais",
            false,
            7),
        new(
            LogLevelKey,
            "Nivel de log do launcher",
            "Nivel de log passado para o crawler Python ao iniciar lotes pela UI.",
            "string",
            "INFO",
            false,
            8)
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
                Category = Category,
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

    public static async Task<Dictionary<string, string>> LoadValuesAsync(ProFinderDbContext context, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultsAsync(context, cancellationToken);

        return await context.AppSettings
            .AsNoTracking()
            .Where(x => x.Category == Category)
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);
    }
}
