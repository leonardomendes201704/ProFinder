using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Settings;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Settings;

namespace ProFinder.Infrastructure.Services;

public class AppSettingService : IAppSettingService
{
    private readonly ProFinderDbContext _context;
    private readonly ILogger<AppSettingService> _logger;

    public AppSettingService(ProFinderDbContext context, ILogger<AppSettingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AppSettingGroupDto>> GetGroupedAsync(CancellationToken cancellationToken = default)
    {
        await CrawlerLauncherSettingCatalog.EnsureDefaultsAsync(_context, cancellationToken);

        var items = await _context.AppSettings
            .AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.DisplayName)
            .Select(x => new AppSettingDto
            {
                Id = x.Id,
                Key = x.Key,
                Category = x.Category,
                DisplayName = x.DisplayName,
                Description = x.Description,
                DataType = x.DataType,
                Value = x.Value,
                DefaultValue = x.DefaultValue,
                IsSensitive = x.IsSensitive,
                IsEditable = x.IsEditable,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return items
            .GroupBy(x => x.Category)
            .Select(group => new AppSettingGroupDto
            {
                Category = group.Key,
                Items = group.ToList()
            })
            .ToList();
    }

    public async Task<AppSettingDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await CrawlerLauncherSettingCatalog.EnsureDefaultsAsync(_context, cancellationToken);

        var entity = await _context.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Configuracao nao encontrada.");

        return new AppSettingDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Category = entity.Category,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            DataType = entity.DataType,
            Value = entity.Value,
            DefaultValue = entity.DefaultValue,
            IsSensitive = entity.IsSensitive,
            IsEditable = entity.IsEditable,
            DisplayOrder = entity.DisplayOrder
        };
    }

    public async Task UpdateAsync(int id, UpdateAppSettingDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.AppSettings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Configuracao nao encontrada.");

        if (!entity.IsEditable)
        {
            throw new ValidationException("Essa configuracao nao pode ser editada.");
        }

        entity.Value = NormalizeValue(entity.DataType, dto.Value);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application setting {SettingKey} updated.", entity.Key);
    }

    private static string NormalizeValue(string dataType, string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        switch (dataType.Trim().ToLowerInvariant())
        {
            case "bool":
            case "boolean":
                if (TryParseBoolean(normalized, out var boolValue))
                {
                    return boolValue ? "true" : "false";
                }

                throw new ValidationException("Informe um valor booleano valido.");

            case "int":
            case "integer":
                if (int.TryParse(normalized, out var intValue))
                {
                    return intValue.ToString(CultureInfo.InvariantCulture);
                }

                throw new ValidationException("Informe um numero inteiro valido.");

            case "decimal":
                if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    return decimalValue.ToString(CultureInfo.InvariantCulture);
                }

                throw new ValidationException("Informe um numero decimal valido.");

            case "json":
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    return "[]";
                }

                try
                {
                    using var _ = JsonDocument.Parse(normalized);
                    return normalized;
                }
                catch (JsonException)
                {
                    throw new ValidationException("Informe um JSON valido.");
                }

            default:
                return normalized;
        }
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        if (bool.TryParse(value, out result))
        {
            return true;
        }

        if (string.Equals(value, "1", StringComparison.Ordinal))
        {
            result = true;
            return true;
        }

        if (string.Equals(value, "0", StringComparison.Ordinal))
        {
            result = false;
            return true;
        }

        if (string.Equals(value, "sim", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (string.Equals(value, "nao", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        result = false;
        return false;
    }
}
