using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Interactions;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Helpers;

namespace ProFinder.Infrastructure.Services;

public class ProfessionalService : IProfessionalService
{
    private readonly ProFinderDbContext _context;
    private readonly ILogger<ProfessionalService> _logger;

    public ProfessionalService(ProFinderDbContext context, ILogger<ProfessionalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ProfessionalListItemDto>> GetPagedAsync(ProfessionalQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Professionals
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.ProfessionalProfessions)
                .ThenInclude(x => x.Profession)
            .Include(x => x.Status)
            .Include(x => x.Source)
            .Include(x => x.ProfessionalRegions)
                .ThenInclude(x => x.Region)
            .AsSplitQuery()
            .AsQueryable();

        if (filter.OnlyActive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();
            query = query.Where(x =>
                x.FullName.Contains(searchTerm) ||
                (x.BusinessName != null && x.BusinessName.Contains(searchTerm)) ||
                (x.Phone != null && x.Phone.Contains(searchTerm)) ||
                (x.WhatsApp != null && x.WhatsApp.Contains(searchTerm)) ||
                (x.Email != null && x.Email.Contains(searchTerm)));
        }

        if (filter.ProfessionId.HasValue)
        {
            query = query.Where(x =>
                x.ProfessionId == filter.ProfessionId.Value ||
                x.ProfessionalProfessions.Any(y => y.ProfessionId == filter.ProfessionId.Value));
        }

        if (filter.StatusId.HasValue)
        {
            query = query.Where(x => x.StatusId == filter.StatusId.Value);
        }

        if (filter.SourceId.HasValue)
        {
            query = query.Where(x => x.SourceId == filter.SourceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(x => x.ProfessionalRegions.Any(region => region.Region != null && region.Region.City == city));
        }

        if (!string.IsNullOrWhiteSpace(filter.Neighborhood))
        {
            var neighborhood = filter.Neighborhood.Trim();
            query = query.Where(x => x.ProfessionalRegions.Any(region => region.Region != null && region.Region.Neighborhood == neighborhood));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var professionals = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProfessionalListItemDto>
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            Items = professionals.Select(MapToListItem).ToList()
        };
    }

    public async Task<ProfessionalDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var professional = await _context.Professionals
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.ProfessionalProfessions)
                .ThenInclude(x => x.Profession)
            .Include(x => x.Status)
            .Include(x => x.Source)
            .Include(x => x.ProfessionalRegions)
                .ThenInclude(x => x.Region)
            .Include(x => x.Interactions)
            .Include(x => x.Evidences)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissional nao encontrado.");

        var professions = BuildProfessionLookups(professional.ProfessionalProfessions, professional.Profession);

        return new ProfessionalDetailsDto
        {
            Id = professional.Id,
            FullName = professional.FullName,
            BusinessName = professional.BusinessName,
            Phone = professional.Phone,
            WhatsApp = professional.WhatsApp,
            Email = professional.Email,
            DocumentNumber = professional.DocumentNumber,
            ProfessionId = professional.ProfessionId,
            ProfessionName = professional.Profession?.Name ?? string.Empty,
            ProfessionNamesDisplay = BuildProfessionNamesDisplay(professions, professional.Profession?.Name),
            Professions = professions,
            SourceId = professional.SourceId,
            SourceName = professional.Source?.Name ?? string.Empty,
            StatusId = professional.StatusId,
            StatusName = professional.Status?.Name ?? string.Empty,
            Notes = professional.Notes,
            Website = professional.Website,
            Instagram = professional.Instagram,
            IsAutonomous = professional.IsAutonomous,
            IsActive = professional.IsActive,
            CreatedAt = professional.CreatedAt,
            UpdatedAt = professional.UpdatedAt,
            Regions = professional.ProfessionalRegions
                .OrderByDescending(x => x.IsPrimaryRegion)
                .ThenBy(x => x.Region!.City)
                .Select(x => new ProfessionalRegionDto
                {
                    RegionId = x.RegionId,
                    RegionDisplayName = BuildRegionDisplay(x.Region),
                    ConfidenceLevel = x.ConfidenceLevel,
                    IsPrimaryRegion = x.IsPrimaryRegion
                })
                .ToList(),
            Interactions = professional.Interactions
                .OrderByDescending(x => x.InteractionDate)
                .Select(MapInteractionToDto)
                .ToList(),
            Evidences = professional.Evidences
                .OrderByDescending(x => x.CollectedAt)
                .Select(x => new EvidenceDto
                {
                    Id = x.Id,
                    FieldName = x.FieldName,
                    FieldValue = x.FieldValue,
                    SourceUrl = x.SourceUrl,
                    CollectedAt = x.CollectedAt
                })
                .ToList()
        };
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Professionals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.FullName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CreateAsync(UpsertProfessionalDto dto, CancellationToken cancellationToken = default)
    {
        var prepared = await ProfessionalWriteSupport.PrepareAsync(_context, dto, null, cancellationToken);
        var professional = ProfessionalWriteSupport.CreateEntity(dto, prepared);

        _context.Professionals.Add(professional);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Professional {ProfessionalId} created.", professional.Id);
        return professional.Id;
    }

    public async Task UpdateAsync(int id, UpsertProfessionalDto dto, CancellationToken cancellationToken = default)
    {
        var prepared = await ProfessionalWriteSupport.PrepareAsync(_context, dto, id, cancellationToken);

        var professional = await _context.Professionals
            .Include(x => x.ProfessionalRegions)
            .Include(x => x.ProfessionalProfessions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissional nao encontrado.");

        ProfessionalWriteSupport.ApplyToExisting(professional, dto, prepared, _context);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Professional {ProfessionalId} updated.", professional.Id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var professional = await _context.Professionals.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissional nao encontrado.");

        professional.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Professional {ProfessionalId} deactivated.", professional.Id);
    }

    private static ProfessionalListItemDto MapToListItem(Professional professional)
    {
        var primaryRegion = professional.ProfessionalRegions
            .OrderByDescending(x => x.IsPrimaryRegion)
            .Select(x => x.Region)
            .FirstOrDefault();
        var professions = BuildProfessionLookups(professional.ProfessionalProfessions, professional.Profession);

        return new ProfessionalListItemDto
        {
            Id = professional.Id,
            FullName = professional.FullName,
            BusinessName = professional.BusinessName,
            Phone = professional.Phone,
            WhatsApp = professional.WhatsApp,
            Email = professional.Email,
            ProfessionName = professional.Profession?.Name ?? string.Empty,
            ProfessionNamesDisplay = BuildProfessionNamesDisplay(professions, professional.Profession?.Name),
            Professions = professions,
            StatusName = professional.Status?.Name ?? string.Empty,
            SourceName = professional.Source?.Name ?? string.Empty,
            PrimaryRegionDisplay = BuildRegionDisplay(primaryRegion),
            IsActive = professional.IsActive,
            CreatedAt = professional.CreatedAt
        };
    }

    private static IReadOnlyList<LookupItemDto> BuildProfessionLookups(
        IEnumerable<ProfessionalProfession> relations,
        Profession? primaryProfession)
    {
        var items = relations
            .Where(x => x.Profession is not null)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Profession!.Name)
            .Select(x => new LookupItemDto
            {
                Id = x.ProfessionId,
                Name = x.Profession!.Name
            })
            .DistinctBy(x => x.Id)
            .ToList();

        if (primaryProfession is not null && items.All(x => x.Id != primaryProfession.Id))
        {
            items.Insert(0, new LookupItemDto
            {
                Id = primaryProfession.Id,
                Name = primaryProfession.Name
            });
        }

        return items;
    }

    private static string BuildProfessionNamesDisplay(IReadOnlyList<LookupItemDto> professions, string? fallbackName)
    {
        if (professions.Count > 0)
        {
            return string.Join(", ", professions.Select(x => x.Name));
        }

        return fallbackName ?? string.Empty;
    }

    private static InteractionDto MapInteractionToDto(Interaction interaction) =>
        new()
        {
            Id = interaction.Id,
            ProfessionalId = interaction.ProfessionalId,
            ProfessionalName = string.Empty,
            InteractionType = interaction.InteractionType,
            InteractionTypeLabel = interaction.InteractionType.ToDisplayLabel(),
            Description = interaction.Description,
            InteractionDate = interaction.InteractionDate,
            CreatedBy = interaction.CreatedBy
        };

    private static string BuildRegionDisplay(Region? region)
    {
        if (region is null)
        {
            return "Nao informado";
        }

        return string.Join(" / ", new[] { region.State, region.City, region.Neighborhood, region.Zone }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
    }

}
