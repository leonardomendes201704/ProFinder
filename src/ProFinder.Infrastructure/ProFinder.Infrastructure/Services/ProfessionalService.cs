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
        var professionIds = GetEffectiveProfessionIds(dto);

        await EnsureLookupReferencesAsync(professionIds, dto.SourceId, dto.StatusId, dto.RegionIds, cancellationToken);
        var normalized = NormalizeProfessional(dto);
        await EnsureProfessionalIsUniqueAsync(normalized.Phone, normalized.WhatsApp, normalized.Email, null, cancellationToken);

        var professional = new Professional
        {
            FullName = normalized.FullName,
            BusinessName = normalized.BusinessName,
            Phone = normalized.Phone,
            WhatsApp = normalized.WhatsApp,
            Email = normalized.Email,
            DocumentNumber = normalized.DocumentNumber,
            ProfessionId = dto.ProfessionId,
            SourceId = dto.SourceId,
            StatusId = dto.StatusId,
            Notes = normalized.Notes,
            Website = normalized.Website,
            Instagram = normalized.Instagram,
            IsAutonomous = dto.IsAutonomous,
            IsActive = dto.IsActive
        };

        ApplyProfessions(professional, professionIds, dto.ProfessionId);
        ApplyRegions(professional, dto.RegionIds, dto.PrimaryRegionId);

        _context.Professionals.Add(professional);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Professional {ProfessionalId} created.", professional.Id);
        return professional.Id;
    }

    public async Task UpdateAsync(int id, UpsertProfessionalDto dto, CancellationToken cancellationToken = default)
    {
        var professionIds = GetEffectiveProfessionIds(dto);

        await EnsureLookupReferencesAsync(professionIds, dto.SourceId, dto.StatusId, dto.RegionIds, cancellationToken);
        var normalized = NormalizeProfessional(dto);
        await EnsureProfessionalIsUniqueAsync(normalized.Phone, normalized.WhatsApp, normalized.Email, id, cancellationToken);

        var professional = await _context.Professionals
            .Include(x => x.ProfessionalRegions)
            .Include(x => x.ProfessionalProfessions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Profissional nao encontrado.");

        professional.FullName = normalized.FullName;
        professional.BusinessName = normalized.BusinessName;
        professional.Phone = normalized.Phone;
        professional.WhatsApp = normalized.WhatsApp;
        professional.Email = normalized.Email;
        professional.DocumentNumber = normalized.DocumentNumber;
        professional.ProfessionId = dto.ProfessionId;
        professional.SourceId = dto.SourceId;
        professional.StatusId = dto.StatusId;
        professional.Notes = normalized.Notes;
        professional.Website = normalized.Website;
        professional.Instagram = normalized.Instagram;
        professional.IsAutonomous = dto.IsAutonomous;
        professional.IsActive = dto.IsActive;

        _context.ProfessionalRegions.RemoveRange(professional.ProfessionalRegions);
        professional.ProfessionalRegions.Clear();
        ApplyRegions(professional, dto.RegionIds, dto.PrimaryRegionId);

        _context.ProfessionalProfessions.RemoveRange(professional.ProfessionalProfessions);
        professional.ProfessionalProfessions.Clear();
        ApplyProfessions(professional, professionIds, dto.ProfessionId);

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

    private async Task EnsureLookupReferencesAsync(
        IReadOnlyCollection<int> professionIds,
        int sourceId,
        int statusId,
        IEnumerable<int> regionIds,
        CancellationToken cancellationToken)
    {
        if (professionIds.Count == 0)
        {
            throw new ValidationException("Selecione ao menos uma profissao.");
        }

        var existingProfessionCount = await _context.Professions.CountAsync(
            x => professionIds.Contains(x.Id) && x.IsActive,
            cancellationToken);
        var sourceExists = await _context.LeadSources.AnyAsync(x => x.Id == sourceId && x.IsActive, cancellationToken);
        var statusExists = await _context.LeadStatuses.AnyAsync(x => x.Id == statusId && x.IsActive, cancellationToken);

        if (existingProfessionCount != professionIds.Count || !sourceExists || !statusExists)
        {
            throw new ValidationException("Profissao, origem ou status invalido.");
        }

        var distinctRegionIds = regionIds.Distinct().ToList();
        if (distinctRegionIds.Count == 0)
        {
            return;
        }

        var existingCount = await _context.Regions.CountAsync(
            x => distinctRegionIds.Contains(x.Id) && x.IsActive,
            cancellationToken);

        if (existingCount != distinctRegionIds.Count)
        {
            throw new ValidationException("Uma ou mais regioes informadas nao existem ou estao inativas.");
        }
    }

    private async Task EnsureProfessionalIsUniqueAsync(
        string? phone,
        string? whatsApp,
        string? email,
        int? currentId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Professionals.AnyAsync(
            x => x.Id != currentId &&
                 x.IsActive &&
                 (
                     (phone != null && (x.Phone == phone || x.WhatsApp == phone)) ||
                     (whatsApp != null && (x.Phone == whatsApp || x.WhatsApp == whatsApp)) ||
                     (email != null && x.Email == email)
                 ),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Ja existe um profissional ativo com o mesmo telefone, WhatsApp ou e-mail.");
        }
    }

    private static (string FullName, string? BusinessName, string? Phone, string? WhatsApp, string? Email, string? DocumentNumber, string? Notes, string? Website, string? Instagram) NormalizeProfessional(UpsertProfessionalDto dto)
    {
        var fullName = dto.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ValidationException("O nome do profissional e obrigatorio.");
        }

        return
        (
            fullName,
            NormalizeOptional(dto.BusinessName),
            PhoneNormalizer.Normalize(dto.Phone),
            PhoneNormalizer.Normalize(dto.WhatsApp),
            NormalizeEmail(dto.Email),
            NormalizeOptional(dto.DocumentNumber),
            NormalizeOptional(dto.Notes),
            NormalizeOptional(dto.Website),
            NormalizeOptional(dto.Instagram)
        );
    }

    private static IReadOnlyList<int> GetEffectiveProfessionIds(UpsertProfessionalDto dto)
    {
        return dto.ProfessionIds
            .Append(dto.ProfessionId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }

    private static void ApplyProfessions(Professional professional, IReadOnlyCollection<int> professionIds, int primaryProfessionId)
    {
        foreach (var professionId in professionIds.Distinct())
        {
            professional.ProfessionalProfessions.Add(new ProfessionalProfession
            {
                ProfessionId = professionId,
                IsPrimary = professionId == primaryProfessionId
            });
        }
    }

    private static void ApplyRegions(Professional professional, IReadOnlyCollection<int> regionIds, int? primaryRegionId)
    {
        var distinctRegionIds = regionIds.Distinct().ToList();

        if (distinctRegionIds.Count == 0)
        {
            return;
        }

        var effectivePrimaryRegionId = primaryRegionId ?? distinctRegionIds.First();

        foreach (var regionId in distinctRegionIds)
        {
            professional.ProfessionalRegions.Add(new ProfessionalRegion
            {
                RegionId = regionId,
                ConfidenceLevel = regionId == effectivePrimaryRegionId ? "Alta" : "Media",
                IsPrimaryRegion = regionId == effectivePrimaryRegionId
            });
        }
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

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
