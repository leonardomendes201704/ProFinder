using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Settings;

namespace ProFinder.Infrastructure.Services;

public class CrawlerRunService : ICrawlerRunService
{
    private static readonly Regex LogLevelRegex = new(@"\[(?<level>[A-Z]+)\]", RegexOptions.Compiled);
    private static readonly HashSet<string> ActiveRunStatuses =
    [
        "Queued",
        "Starting",
        "Running",
        "Stopping"
    ];
    private static readonly HashSet<string> SupportedSites =
    [
        "google_maps",
        "olx",
        "telelistas",
        "guiamais"
    ];

    private readonly ProFinderDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CrawlerRunService> _logger;
    private readonly CrawlerProcessRegistry _processRegistry;
    private readonly ICrawlerRunRealtimeNotifier _realtimeNotifier;

    public CrawlerRunService(
        ProFinderDbContext context,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<CrawlerRunService> logger,
        CrawlerProcessRegistry processRegistry,
        ICrawlerRunRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _processRegistry = processRegistry;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<PagedResult<CrawlerRunListItemDto>> GetPagedAsync(CrawlerRunQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.LeadCaptureRuns
            .AsNoTracking()
            .Where(x => x.CaptureType == "MultiSiteCrawler");

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();
            query = query.Where(x =>
                (x.SearchQuery != null && x.SearchQuery.Contains(searchTerm)) ||
                (x.CreatedBy != null && x.CreatedBy.Contains(searchTerm)) ||
                (x.Notes != null && x.Notes.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new CrawlerRunListItemDto
            {
                Id = x.Id,
                CaptureType = x.CaptureType,
                Status = x.Status,
                SearchQuery = x.SearchQuery,
                Notes = x.Notes,
                ErrorMessage = x.ErrorMessage,
                CreatedBy = x.CreatedBy,
                ItemsCaptured = x.ItemsCaptured,
                ItemsInserted = x.ItemsInserted,
                ItemsUpdated = x.ItemsUpdated,
                ItemsSkipped = x.ItemsSkipped,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<CrawlerRunListItemDto>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CrawlerRunDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await BuildRunDetailsAsync(_context, id, cancellationToken)
            ?? throw new NotFoundException("Lote do crawler nao encontrado.");
    }

    public async Task<IReadOnlyList<CrawlerRunLogDto>> GetLogsAsync(int runId, int take = 200, CancellationToken cancellationToken = default)
    {
        var runExists = await _context.LeadCaptureRuns
            .AsNoTracking()
            .AnyAsync(x => x.Id == runId && x.CaptureType == "MultiSiteCrawler", cancellationToken);

        if (!runExists)
        {
            throw new NotFoundException("Lote do crawler nao encontrado.");
        }

        var items = await _context.LeadCaptureLogs
            .AsNoTracking()
            .Where(x => x.LeadCaptureRunId == runId)
            .OrderByDescending(x => x.Id)
            .Take(take)
            .Select(x => new CrawlerRunLogDto
            {
                Id = x.Id,
                LeadCaptureRunId = x.LeadCaptureRunId,
                LogLevel = x.LogLevel,
                Source = x.Source,
                Message = x.Message,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        items.Reverse();
        return items;
    }

    public async Task<CrawlerLauncherDefaultsDto> GetLauncherDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await CrawlerRuntimeSettingCatalog.EnsureDefaultsAsync(_context, cancellationToken);
        var values = await CrawlerLauncherSettingCatalog.LoadValuesAsync(_context, cancellationToken);
        return new CrawlerLauncherDefaultsDto
        {
            PythonExecutable = GetSetting(values, CrawlerLauncherSettingCatalog.PythonExecutableKey, "python"),
            ScriptPath = GetSetting(values, CrawlerLauncherSettingCatalog.ScriptPathKey, "crawler/main.py"),
            WorkingDirectory = GetSetting(values, CrawlerLauncherSettingCatalog.WorkingDirectoryKey, string.Empty),
            ExportDirectory = GetSetting(values, CrawlerLauncherSettingCatalog.ExportDirectoryKey, "crawler/exports"),
            DefaultSitesCsv = GetSetting(values, CrawlerLauncherSettingCatalog.DefaultSitesKey, "google_maps,olx,telelistas,guiamais"),
            LogLevel = GetSetting(values, CrawlerLauncherSettingCatalog.LogLevelKey, "INFO"),
            SqlDriver = GetSetting(values, CrawlerLauncherSettingCatalog.SqlDriverKey, "ODBC Driver 18 for SQL Server"),
            HasConnectionStringOverride = !string.IsNullOrWhiteSpace(GetSetting(values, CrawlerLauncherSettingCatalog.ConnectionStringOverrideKey, string.Empty))
        };
    }

    public async Task<LeadCaptureRunDto> QueueAsync(StartCrawlerRunRequestDto dto, CancellationToken cancellationToken = default)
    {
        ValidateRequest(dto);

        await CrawlerRuntimeSettingCatalog.EnsureDefaultsAsync(_context, cancellationToken);
        var launcherSettings = await CrawlerLauncherSettingCatalog.LoadValuesAsync(_context, cancellationToken);
        var requestedBy = string.IsNullOrWhiteSpace(dto.RequestedBy) ? "AdminUI" : dto.RequestedBy.Trim();
        var searchQuery = BuildSearchQuery(dto.Service, dto.City);
        var selectedSites = dto.SelectedSites
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(SupportedSites.Contains)
            .Distinct()
            .ToList();

        var run = new LeadCaptureRun
        {
            CaptureType = "MultiSiteCrawler",
            Status = "Queued",
            SearchQuery = searchQuery,
            Notes = BuildNotes(dto, selectedSites),
            CreatedBy = requestedBy,
            StartedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        _context.LeadCaptureRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);

        await PersistLogAsync(run.Id, "Info", "launcher", $"Lote enfileirado para '{searchQuery}'.", cancellationToken);

        try
        {
            var process = StartProcess(run.Id, dto, selectedSites, requestedBy, launcherSettings);
            _processRegistry.Register(run.Id, process);

            run.Status = "Starting";
            await _context.SaveChangesAsync(cancellationToken);
            await PublishRunUpdateAsync(run.Id, cancellationToken);
            await PersistLogAsync(run.Id, "Info", "launcher", "Processo Python iniciado pelo admin.", cancellationToken);

            _ = Task.Run(() => MonitorProcessAsync(process, run.Id));

            _logger.LogInformation("Crawler UI launch queued. RunId: {RunId}", run.Id);
        }
        catch (Exception exception)
        {
            run.Status = "Failed";
            run.CompletedAt = DateTime.UtcNow;
            run.ErrorMessage = Truncate(exception.Message, 2000);
            await _context.SaveChangesAsync(cancellationToken);
            await PersistLogAsync(run.Id, "Error", "launcher", $"Falha ao iniciar o processo Python: {exception.Message}", cancellationToken);
            await PublishRunUpdateAsync(run.Id, cancellationToken);
            _logger.LogError(exception, "Unable to start crawler UI run {RunId}.", run.Id);
            throw new ValidationException("Nao foi possivel iniciar o processo do crawler. Verifique as configuracoes do launcher.");
        }

        return MapToRunDto(run);
    }

    public async Task<LeadCaptureRunDto> RequestStopAsync(int runId, string requestedBy, CancellationToken cancellationToken = default)
    {
        var run = await _context.LeadCaptureRuns
            .FirstOrDefaultAsync(x => x.Id == runId && x.CaptureType == "MultiSiteCrawler", cancellationToken)
            ?? throw new NotFoundException("Lote do crawler nao encontrado.");

        if (run.Status is "Completed" or "Failed" or "Stopped")
        {
            throw new ValidationException("Esse lote ja foi finalizado e nao pode mais ser interrompido.");
        }

        if (run.Status == "Stopping")
        {
            throw new ValidationException("A parada deste lote ja foi solicitada.");
        }

        var actor = string.IsNullOrWhiteSpace(requestedBy) ? "AdminUI" : requestedBy.Trim();
        run.Status = "Stopping";
        run.Notes = Truncate($"{run.Notes}{Environment.NewLine}Parada solicitada por {actor} em {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC.", 1000);
        await _context.SaveChangesAsync(cancellationToken);

        await PersistLogAsync(runId, "Warning", "launcher", $"Parada solicitada por {actor}.", cancellationToken);
        await PublishRunUpdateAsync(runId, cancellationToken);

        if (_processRegistry.TryGet(runId, out var process) && process is not null)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await PersistLogAsync(runId, "Warning", "launcher", "Processo Python encerrado localmente pelo launcher.", cancellationToken);
                }
            }
            catch (Exception exception)
            {
                await PersistLogAsync(runId, "Error", "launcher", $"Falha ao encerrar o processo localmente: {exception.Message}", cancellationToken);
                _logger.LogWarning(exception, "Unable to kill crawler process for RunId {RunId}.", runId);
            }
        }

        return MapToRunDto(run);
    }

    public async Task<CrawlerDataResetResultDto> ResetDataAsync(string requestedBy, CancellationToken cancellationToken = default)
    {
        var actor = string.IsNullOrWhiteSpace(requestedBy) ? "AdminUI" : requestedBy.Trim();
        var localProcesses = _processRegistry.Snapshot();
        var localRunIds = localProcesses
            .Select(x => x.Key)
            .ToHashSet();

        var activeExternalRunIds = await _context.LeadCaptureRuns
            .AsNoTracking()
            .Where(x => ActiveRunStatuses.Contains(x.Status) && !localRunIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (activeExternalRunIds.Count > 0)
        {
            throw new ValidationException(
                $"Nao e possivel resetar enquanto existem capturas ativas em outra execucao: {string.Join(", ", activeExternalRunIds.Select(x => $"#{x}"))}. Pare esses lotes antes de limpar os dados.");
        }

        await StopLocalProcessesAsync(localProcesses);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var providerLeadCount = await _context.ProviderLeads.ExecuteDeleteAsync(cancellationToken);
        var googleMapsLeadCount = await _context.GoogleMapsLeads.ExecuteDeleteAsync(cancellationToken);
        var leadCaptureLogCount = await _context.LeadCaptureLogs.ExecuteDeleteAsync(cancellationToken);
        var leadCaptureRunCount = await _context.LeadCaptureRuns.ExecuteDeleteAsync(cancellationToken);

        await ReseedIdentityAsync(TableNames.ProviderLeads, cancellationToken);
        await ReseedIdentityAsync(TableNames.GoogleMapsLeads, cancellationToken);
        await ReseedIdentityAsync(TableNames.LeadCaptureLogs, cancellationToken);
        await ReseedIdentityAsync(TableNames.LeadCaptureRuns, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var result = new CrawlerDataResetResultDto
        {
            LeadCaptureRunCount = leadCaptureRunCount,
            ProviderLeadCount = providerLeadCount,
            GoogleMapsLeadCount = googleMapsLeadCount,
            LeadCaptureLogCount = leadCaptureLogCount,
            RequestedBy = actor,
            OccurredAtUtc = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Crawler data reset by {RequestedBy}. Runs: {RunCount}, ProviderLeads: {ProviderLeadCount}, GoogleMapsLeads: {GoogleMapsLeadCount}, Logs: {LogCount}",
            actor,
            leadCaptureRunCount,
            providerLeadCount,
            googleMapsLeadCount,
            leadCaptureLogCount);

        await _realtimeNotifier.NotifyDataResetAsync(result, cancellationToken);
        return result;
    }

    private Process StartProcess(
        int runId,
        StartCrawlerRunRequestDto dto,
        IReadOnlyList<string> selectedSites,
        string requestedBy,
        IReadOnlyDictionary<string, string> launcherSettings)
    {
        var solutionRoot = ResolveSolutionRoot();
        var workingDirectory = ResolvePath(
            GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.WorkingDirectoryKey, string.Empty),
            solutionRoot,
            allowEmpty: true);

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            workingDirectory = solutionRoot;
        }

        var scriptPath = ResolvePath(
            GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.ScriptPathKey, "crawler/main.py"),
            workingDirectory,
            allowEmpty: false);

        if (!File.Exists(scriptPath))
        {
            throw new InvalidOperationException($"Arquivo do crawler nao encontrado em '{scriptPath}'.");
        }

        var pythonExecutable = GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.PythonExecutableKey, "python");
        var exportDirectory = ResolvePath(
            GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.ExportDirectoryKey, "crawler/exports"),
            workingDirectory,
            allowEmpty: false);
        var logLevel = GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.LogLevelKey, "INFO");
        var connectionString = BuildOdbcConnectionString(launcherSettings);

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("--run-id");
        startInfo.ArgumentList.Add(runId.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--city");
        startInfo.ArgumentList.Add(dto.City.Trim());
        startInfo.ArgumentList.Add("--service");
        startInfo.ArgumentList.Add(dto.Service.Trim());
        startInfo.ArgumentList.Add("--sites");
        startInfo.ArgumentList.Add(string.Join(",", selectedSites));
        startInfo.ArgumentList.Add("--created-by");
        startInfo.ArgumentList.Add(requestedBy);
        startInfo.ArgumentList.Add("--connection-string");
        startInfo.ArgumentList.Add(connectionString);
        startInfo.ArgumentList.Add("--export-dir");
        startInfo.ArgumentList.Add(exportDirectory);
        startInfo.ArgumentList.Add("--log-level");
        startInfo.ArgumentList.Add(logLevel);

        if (dto.ProfessionId.HasValue)
        {
            startInfo.ArgumentList.Add("--profession-id");
            startInfo.ArgumentList.Add(dto.ProfessionId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (dto.RegionId.HasValue)
        {
            startInfo.ArgumentList.Add("--region-id");
            startInfo.ArgumentList.Add(dto.RegionId.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (dto.MaxRecords.HasValue)
        {
            startInfo.ArgumentList.Add("--max-records");
            startInfo.ArgumentList.Add(dto.MaxRecords.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (string.Equals(dto.HeadlessMode, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(dto.HeadlessMode, "false", StringComparison.OrdinalIgnoreCase))
        {
            var headlessMode = dto.HeadlessMode?.Trim().ToLowerInvariant() ?? "true";
            startInfo.ArgumentList.Add("--headless");
            startInfo.ArgumentList.Add(headlessMode);
        }

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("O processo Python nao iniciou.");
        }

        return process;
    }

    private async Task MonitorProcessAsync(Process process, int runId)
    {
        try
        {
            var stdoutTask = ReadOutputStreamAsync(process.StandardOutput, runId, "stdout");
            var stderrTask = ReadOutputStreamAsync(process.StandardError, runId, "stderr");

            await process.WaitForExitAsync();
            await Task.WhenAll(stdoutTask, stderrTask);

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProFinderDbContext>();
            var run = await context.LeadCaptureRuns.FirstOrDefaultAsync(x => x.Id == runId);
            if (run is null)
            {
                return;
            }

            if (run.Status == "Stopping")
            {
                run.Status = "Stopped";
                run.CompletedAt = DateTime.UtcNow;
                run.ErrorMessage = null;
                await context.SaveChangesAsync();
                await PersistLogAsync(runId, "Warning", "launcher", "Lote encerrado por solicitacao do usuario.");
            }
            else if (process.ExitCode != 0 && run.Status is not ("Completed" or "Failed" or "Stopped"))
            {
                run.Status = "Failed";
                run.CompletedAt = DateTime.UtcNow;
                run.ErrorMessage = Truncate("O processo Python foi encerrado com erro. Consulte o console do lote.", 2000);
                await context.SaveChangesAsync();
                await PersistLogAsync(runId, "Error", "launcher", $"Processo Python encerrado com codigo {process.ExitCode}.");
            }
            else if (process.ExitCode == 0 && run.Status is "Queued" or "Starting" or "Running")
            {
                run.Status = "Completed";
                run.CompletedAt = DateTime.UtcNow;
                run.Notes = Truncate($"{run.Notes}{Environment.NewLine}Processo finalizado sem retorno adicional do launcher.", 1000);
                await context.SaveChangesAsync();
            }

            await PublishRunUpdateAsync(runId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Crawler process monitor failed for RunId {RunId}.", runId);
            await PersistLogAsync(runId, "Error", "launcher", $"Falha no monitor do processo: {exception.Message}");
        }
        finally
        {
            _processRegistry.Remove(runId);
            process.Dispose();
        }
    }

    private async Task ReadOutputStreamAsync(StreamReader reader, int runId, string source)
    {
        while (true)
        {
            string? line;
            try
            {
                line = await reader.ReadLineAsync();
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (line is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var (logLevel, message) = ParseLogLine(line, source);
            await PersistLogAsync(runId, logLevel, source, message);
        }
    }

    private async Task PersistLogAsync(
        int runId,
        string logLevel,
        string source,
        string message,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProFinderDbContext>();

        var entry = new LeadCaptureLog
        {
            LeadCaptureRunId = runId,
            LogLevel = Truncate(logLevel, 20) ?? "Info",
            Source = Truncate(source, 30) ?? "process",
            Message = Truncate(message, 4000) ?? string.Empty
        };

        context.LeadCaptureLogs.Add(entry);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var runExists = await context.LeadCaptureRuns
                .AsNoTracking()
                .AnyAsync(x => x.Id == runId, cancellationToken);

            if (!runExists)
            {
                return;
            }

            throw;
        }

        var logDto = new CrawlerRunLogDto
        {
            Id = entry.Id,
            LeadCaptureRunId = entry.LeadCaptureRunId,
            LogLevel = entry.LogLevel,
            Source = entry.Source,
            Message = entry.Message,
            CreatedAt = entry.CreatedAt
        };

        await _realtimeNotifier.NotifyLogAddedAsync(logDto, cancellationToken);
        await PublishRunUpdateAsync(runId, cancellationToken, context);
    }

    private async Task StopLocalProcessesAsync(IReadOnlyList<KeyValuePair<int, Process>> processes)
    {
        foreach (var item in processes)
        {
            try
            {
                if (!item.Value.HasExited)
                {
                    item.Value.Kill(entireProcessTree: true);
                    item.Value.WaitForExit(5000);
                }

                if (!item.Value.HasExited)
                {
                    throw new InvalidOperationException($"O processo do lote #{item.Key} nao encerrou no tempo esperado.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to stop local crawler process for RunId {RunId} during reset.", item.Key);
                throw new ValidationException($"Nao foi possivel interromper o processo local do lote #{item.Key}. Pare o lote manualmente e tente novamente.");
            }
            finally
            {
                _processRegistry.Remove(item.Key);
            }
        }

        await Task.CompletedTask;
    }

    private Task ReseedIdentityAsync(string tableName, CancellationToken cancellationToken)
    {
        var sql = tableName switch
        {
            TableNames.ProviderLeads => "DBCC CHECKIDENT ('prf_provider_leads', RESEED, 0);",
            TableNames.GoogleMapsLeads => "DBCC CHECKIDENT ('prf_google_maps_leads', RESEED, 0);",
            TableNames.LeadCaptureLogs => "DBCC CHECKIDENT ('prf_lead_capture_logs', RESEED, 0);",
            TableNames.LeadCaptureRuns => "DBCC CHECKIDENT ('prf_lead_capture_runs', RESEED, 0);",
            _ => throw new InvalidOperationException($"Tabela nao suportada para reseed: {tableName}.")
        };

        return _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task PublishRunUpdateAsync(int runId, CancellationToken cancellationToken = default, ProFinderDbContext? existingContext = null)
    {
        if (existingContext is not null)
        {
            var details = await BuildRunDetailsAsync(existingContext, runId, cancellationToken);
            if (details is not null)
            {
                await _realtimeNotifier.NotifyRunUpdatedAsync(details, cancellationToken);
            }

            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProFinderDbContext>();
        var run = await BuildRunDetailsAsync(context, runId, cancellationToken);
        if (run is not null)
        {
            await _realtimeNotifier.NotifyRunUpdatedAsync(run, cancellationToken);
        }
    }

    private static async Task<CrawlerRunDetailsDto?> BuildRunDetailsAsync(
        ProFinderDbContext context,
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await context.LeadCaptureRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CaptureType == "MultiSiteCrawler", cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var providerLeadCount = await context.ProviderLeads
            .AsNoTracking()
            .CountAsync(x => x.LeadCaptureRunId == id, cancellationToken);

        return new CrawlerRunDetailsDto
        {
            Id = entity.Id,
            LeadSourceId = entity.LeadSourceId,
            CaptureType = entity.CaptureType,
            Status = entity.Status,
            SourceUrl = entity.SourceUrl,
            FileName = entity.FileName,
            SearchQuery = entity.SearchQuery,
            Notes = entity.Notes,
            ErrorMessage = entity.ErrorMessage,
            CreatedBy = entity.CreatedBy,
            ItemsCaptured = entity.ItemsCaptured,
            ItemsInserted = entity.ItemsInserted,
            ItemsUpdated = entity.ItemsUpdated,
            ItemsSkipped = entity.ItemsSkipped,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            ProviderLeadCount = providerLeadCount,
            CanStop = entity.Status is "Queued" or "Starting" or "Running" or "Stopping"
        };
    }

    private string BuildOdbcConnectionString(IReadOnlyDictionary<string, string> launcherSettings)
    {
        var overrideConnectionString = GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.ConnectionStringOverrideKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(overrideConnectionString))
        {
            return overrideConnectionString.Trim();
        }

        var applicationConnectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(applicationConnectionString))
        {
            throw new InvalidOperationException("Connection string DefaultConnection nao configurada.");
        }

        var sqlBuilder = new SqlConnectionStringBuilder(applicationConnectionString);
        var parts = new List<string>
        {
            $"Driver={WrapOdbcValue(GetSetting(launcherSettings, CrawlerLauncherSettingCatalog.SqlDriverKey, "ODBC Driver 18 for SQL Server"))}",
            $"Server={WrapOdbcValue(sqlBuilder.DataSource)}",
            $"Database={WrapOdbcValue(sqlBuilder.InitialCatalog)}",
            $"Encrypt={(sqlBuilder.Encrypt ? "yes" : "no")}",
            $"TrustServerCertificate={(sqlBuilder.TrustServerCertificate ? "yes" : "no")}",
            $"Connection Timeout={sqlBuilder.ConnectTimeout.ToString(CultureInfo.InvariantCulture)}"
        };

        if (sqlBuilder.IntegratedSecurity)
        {
            parts.Add("Trusted_Connection=Yes");
        }
        else
        {
            parts.Add($"Uid={WrapOdbcValue(sqlBuilder.UserID)}");
            parts.Add($"Pwd={WrapOdbcValue(sqlBuilder.Password)}");
        }

        return string.Join(";", parts) + ";";
    }

    private string ResolveSolutionRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "ProFinder.sln")) ||
                File.Exists(Path.Combine(current.FullName, "crawler", "main.py")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private static string ResolvePath(string pathValue, string baseDirectory, bool allowEmpty)
    {
        var trimmed = pathValue.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return allowEmpty ? string.Empty : Path.GetFullPath(baseDirectory);
        }

        return Path.IsPathRooted(trimmed)
            ? trimmed
            : Path.GetFullPath(Path.Combine(baseDirectory, trimmed));
    }

    private static void ValidateRequest(StartCrawlerRunRequestDto dto)
    {
        if (dto.SelectedSites.Count == 0)
        {
            throw new ValidationException("Selecione pelo menos um site para o lote.");
        }

        var invalidSites = dto.SelectedSites
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !SupportedSites.Contains(x))
            .Distinct()
            .ToList();

        if (invalidSites.Count > 0)
        {
            throw new ValidationException($"Sites nao suportados: {string.Join(", ", invalidSites)}");
        }
    }

    private static string BuildNotes(StartCrawlerRunRequestDto dto, IReadOnlyList<string> selectedSites)
    {
        var headlessLabel = dto.HeadlessMode switch
        {
            "true" => "Forcado em headless",
            "false" => "Forcado em modo visivel",
            _ => "Usando configuracao padrao"
        };

        return Truncate(
            $"Cidade: {dto.City.Trim()} | Servico: {dto.Service.Trim()} | Sites: {string.Join(", ", selectedSites)} | {headlessLabel} | MaxRecords: {(dto.MaxRecords?.ToString(CultureInfo.InvariantCulture) ?? "padrao")}",
            1000) ?? string.Empty;
    }

    private static LeadCaptureRunDto MapToRunDto(LeadCaptureRun run) =>
        new()
        {
            Id = run.Id,
            LeadSourceId = run.LeadSourceId,
            CaptureType = run.CaptureType,
            Status = run.Status,
            SourceUrl = run.SourceUrl,
            FileName = run.FileName,
            SearchQuery = run.SearchQuery,
            Notes = run.Notes,
            ErrorMessage = run.ErrorMessage,
            ItemsCaptured = run.ItemsCaptured,
            ItemsInserted = run.ItemsInserted,
            ItemsUpdated = run.ItemsUpdated,
            ItemsSkipped = run.ItemsSkipped,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt
        };

    private static string BuildSearchQuery(string service, string city) => Truncate($"{service.Trim()} {city.Trim()}", 200) ?? string.Empty;

    private static (string LogLevel, string Message) ParseLogLine(string line, string source)
    {
        var match = LogLevelRegex.Match(line);
        if (match.Success)
        {
            return (NormalizeLogLevel(match.Groups["level"].Value), line.Trim());
        }

        return (source == "stderr" ? "Error" : "Info", line.Trim());
    }

    private static string NormalizeLogLevel(string level)
    {
        return level.Trim().ToUpperInvariant() switch
        {
            "DEBUG" => "Debug",
            "WARNING" => "Warning",
            "WARN" => "Warning",
            "ERROR" => "Error",
            "CRITICAL" => "Error",
            _ => "Info"
        };
    }

    private static string WrapOdbcValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Contains(';', StringComparison.Ordinal) || value.Contains('{', StringComparison.Ordinal) || value.Contains('}', StringComparison.Ordinal)
            ? "{" + value.Replace("}", "}}", StringComparison.Ordinal) + "}"
            : value;
    }

    private static string GetSetting(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.TryGetValue(key, out var value) ? value : fallback;

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
