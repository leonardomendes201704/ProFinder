using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Professions;
using ProFinder.Application.DTOs.Regions;
using ProFinder.Application.DTOs.Sources;
using ProFinder.Application.DTOs.Statuses;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly ProFinderDbContext _context;
    private readonly ILogger<LookupService> _logger;

    public LookupService(ProFinderDbContext context, ILogger<LookupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProfessionDto>> GetProfessionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Professions.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new ProfessionDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProfessionDto> GetProfessionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var profession = await _context.Professions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissão não encontrada.");

        return new ProfessionDto
        {
            Id = profession.Id,
            Name = profession.Name,
            Description = profession.Description,
            IsActive = profession.IsActive
        };
    }

    public async Task<int> CreateProfessionAsync(UpsertProfessionDto dto, CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequired(dto.Name);
        await EnsureProfessionNameIsAvailableAsync(name, null, cancellationToken);

        var entity = new Profession
        {
            Name = name,
            Description = NormalizeOptional(dto.Description),
            IsActive = dto.IsActive
        };

        _context.Professions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Profession {ProfessionId} created.", entity.Id);
        return entity.Id;
    }

    public async Task UpdateProfessionAsync(int id, UpsertProfessionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Professions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissão não encontrada.");

        var name = NormalizeRequired(dto.Name);
        await EnsureProfessionNameIsAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        entity.Description = NormalizeOptional(dto.Description);
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Profession {ProfessionId} updated.", entity.Id);
    }

    public async Task DeleteProfessionAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Professions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissão não encontrada.");

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Profession {ProfessionId} deactivated.", entity.Id);
    }

    public async Task<IReadOnlyList<RegionDto>> GetRegionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Regions.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.State)
            .ThenBy(x => x.City)
            .ThenBy(x => x.Neighborhood)
            .Select(x => new RegionDto
            {
                Id = x.Id,
                State = x.State,
                City = x.City,
                Neighborhood = x.Neighborhood,
                ZipCode = x.ZipCode,
                Zone = x.Zone,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                RadiusKm = x.RadiusKm,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RegionDto> GetRegionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var region = await _context.Regions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Região não encontrada.");

        return new RegionDto
        {
            Id = region.Id,
            State = region.State,
            City = region.City,
            Neighborhood = region.Neighborhood,
            ZipCode = region.ZipCode,
            Zone = region.Zone,
            Latitude = region.Latitude,
            Longitude = region.Longitude,
            RadiusKm = region.RadiusKm,
            IsActive = region.IsActive
        };
    }

    public async Task<int> CreateRegionAsync(UpsertRegionDto dto, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRegion(dto);
        await EnsureRegionIsAvailableAsync(normalized.State, normalized.City, normalized.Neighborhood, normalized.ZipCode, null, cancellationToken);

        var entity = new Region
        {
            State = normalized.State,
            City = normalized.City,
            Neighborhood = normalized.Neighborhood,
            ZipCode = normalized.ZipCode,
            Zone = normalized.Zone,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            RadiusKm = dto.RadiusKm,
            IsActive = dto.IsActive
        };

        _context.Regions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Region {RegionId} created.", entity.Id);
        return entity.Id;
    }

    public async Task UpdateRegionAsync(int id, UpsertRegionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Regions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Região não encontrada.");

        var normalized = NormalizeRegion(dto);
        await EnsureRegionIsAvailableAsync(normalized.State, normalized.City, normalized.Neighborhood, normalized.ZipCode, id, cancellationToken);

        entity.State = normalized.State;
        entity.City = normalized.City;
        entity.Neighborhood = normalized.Neighborhood;
        entity.ZipCode = normalized.ZipCode;
        entity.Zone = normalized.Zone;
        entity.Latitude = dto.Latitude;
        entity.Longitude = dto.Longitude;
        entity.RadiusKm = dto.RadiusKm;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Region {RegionId} updated.", entity.Id);
    }

    public async Task DeleteRegionAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Regions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Região não encontrada.");

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Region {RegionId} deactivated.", entity.Id);
    }

    public async Task<IReadOnlyList<LeadSourceDto>> GetSourcesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.LeadSources.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new LeadSourceDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Url = x.Url,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadSourceDto> GetSourceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await _context.LeadSources.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem não encontrada.");

        return new LeadSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Url = source.Url,
            IsActive = source.IsActive
        };
    }

    public async Task<int> CreateSourceAsync(UpsertLeadSourceDto dto, CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequired(dto.Name);
        await EnsureSourceNameIsAvailableAsync(name, null, cancellationToken);

        var entity = new LeadSource
        {
            Name = name,
            Description = NormalizeOptional(dto.Description),
            Url = NormalizeOptional(dto.Url),
            IsActive = dto.IsActive
        };

        _context.LeadSources.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Lead source {LeadSourceId} created.", entity.Id);
        return entity.Id;
    }

    public async Task UpdateSourceAsync(int id, UpsertLeadSourceDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LeadSources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem não encontrada.");

        var name = NormalizeRequired(dto.Name);
        await EnsureSourceNameIsAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        entity.Description = NormalizeOptional(dto.Description);
        entity.Url = NormalizeOptional(dto.Url);
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lead source {LeadSourceId} updated.", entity.Id);
    }

    public async Task DeleteSourceAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LeadSources.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem não encontrada.");

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lead source {LeadSourceId} deactivated.", entity.Id);
    }

    public async Task<IReadOnlyList<LeadStatusDto>> GetStatusesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.LeadStatuses.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        return await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new LeadStatusDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadStatusDto> GetStatusByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var status = await _context.LeadStatuses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Status não encontrado.");

        return new LeadStatusDto
        {
            Id = status.Id,
            Name = status.Name,
            Description = status.Description,
            DisplayOrder = status.DisplayOrder,
            IsActive = status.IsActive
        };
    }

    public async Task<int> CreateStatusAsync(UpsertLeadStatusDto dto, CancellationToken cancellationToken = default)
    {
        var name = NormalizeRequired(dto.Name);
        await EnsureStatusNameIsAvailableAsync(name, null, cancellationToken);

        var entity = new LeadStatus
        {
            Name = name,
            Description = NormalizeOptional(dto.Description),
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.LeadStatuses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Lead status {LeadStatusId} created.", entity.Id);
        return entity.Id;
    }

    public async Task UpdateStatusAsync(int id, UpsertLeadStatusDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LeadStatuses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Status não encontrado.");

        var name = NormalizeRequired(dto.Name);
        await EnsureStatusNameIsAvailableAsync(name, id, cancellationToken);

        entity.Name = name;
        entity.Description = NormalizeOptional(dto.Description);
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lead status {LeadStatusId} updated.", entity.Id);
    }

    public async Task DeleteStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.LeadStatuses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Status não encontrado.");

        entity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Lead status {LeadStatusId} deactivated.", entity.Id);
    }

    private async Task EnsureProfessionNameIsAvailableAsync(string name, int? currentId, CancellationToken cancellationToken)
    {
        var exists = await _context.Professions.AnyAsync(
            x => x.Id != currentId && x.Name.ToLower() == name.ToLower(),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Já existe uma profissão com esse nome.");
        }
    }

    private async Task EnsureSourceNameIsAvailableAsync(string name, int? currentId, CancellationToken cancellationToken)
    {
        var exists = await _context.LeadSources.AnyAsync(
            x => x.Id != currentId && x.Name.ToLower() == name.ToLower(),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Já existe uma origem com esse nome.");
        }
    }

    private async Task EnsureStatusNameIsAvailableAsync(string name, int? currentId, CancellationToken cancellationToken)
    {
        var exists = await _context.LeadStatuses.AnyAsync(
            x => x.Id != currentId && x.Name.ToLower() == name.ToLower(),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Já existe um status com esse nome.");
        }
    }

    private async Task EnsureRegionIsAvailableAsync(
        string state,
        string city,
        string? neighborhood,
        string? zipCode,
        int? currentId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Regions.AnyAsync(
            x => x.Id != currentId &&
                 x.State == state &&
                 x.City.ToLower() == city.ToLower() &&
                 ((x.Neighborhood ?? string.Empty).ToLower() == (neighborhood ?? string.Empty).ToLower()) &&
                 ((x.ZipCode ?? string.Empty) == (zipCode ?? string.Empty)),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Já existe uma região com os mesmos dados principais.");
        }
    }

    private static string NormalizeRequired(string value)
    {
        var normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ValidationException("Preencha os campos obrigatórios.")
            : normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static (string State, string City, string? Neighborhood, string? ZipCode, string? Zone) NormalizeRegion(UpsertRegionDto dto)
    {
        var state = NormalizeRequired(dto.State).ToUpperInvariant();
        var city = NormalizeRequired(dto.City);
        var neighborhood = NormalizeOptional(dto.Neighborhood);
        var zipCode = NormalizeOptional(dto.ZipCode);
        var zone = NormalizeOptional(dto.Zone);

        return (state, city, neighborhood, zipCode, zone);
    }
}
