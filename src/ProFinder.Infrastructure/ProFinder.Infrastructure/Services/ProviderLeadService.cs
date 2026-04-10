using Microsoft.EntityFrameworkCore;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class ProviderLeadService : IProviderLeadService
{
    private readonly ProFinderDbContext _context;

    public ProviderLeadService(ProFinderDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProviderLeadListItemDto>> GetPagedAsync(ProviderLeadQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var filteredQuery = BuildFilteredQuery(filter);
        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var items = await filteredQuery
            .Include(x => x.Profession)
            .Include(x => x.ProviderLeadProfessions)
                .ThenInclude(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .Include(x => x.ImportedProfessional)
            .AsSplitQuery()
            .OrderByDescending(x => x.ScrapedAt)
            .ThenByDescending(x => x.Id)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProviderLeadListItemDto>
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            Items = items.Select(MapToListItem).ToList()
        };
    }

    public async Task<ProviderLeadMapResultDto> GetMapItemsAsync(ProviderLeadQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var filteredQuery = BuildFilteredQuery(filter);
        var totalFiltered = await filteredQuery.CountAsync(cancellationToken);

        var items = await filteredQuery
            .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .OrderByDescending(x => x.ScrapedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return new ProviderLeadMapResultDto
        {
            TotalFiltered = totalFiltered,
            TotalMapped = items.Count,
            Items = items.Select(MapToMapItem).ToList()
        };
    }

    public async Task<IReadOnlyList<ProviderLeadExportItemDto>> GetExportItemsAsync(ProviderLeadQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var items = await BuildFilteredQuery(filter)
            .Include(x => x.Profession)
            .Include(x => x.ProviderLeadProfessions)
                .ThenInclude(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .Include(x => x.ImportedProfessional)
            .AsSplitQuery()
            .OrderByDescending(x => x.ScrapedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return items.Select(MapToExportItem).ToList();
    }

    public async Task<ProviderLeadDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ProviderLeads
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.ProviderLeadProfessions)
                .ThenInclude(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .Include(x => x.ImportedProfessional)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Lead capturado nao encontrado.");

        var professions = BuildProfessionLookups(entity.ProviderLeadProfessions, entity.Profession);

        return new ProviderLeadDetailsDto
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            LeadSourceId = entity.LeadSourceId,
            ProfessionId = entity.ProfessionId,
            RegionId = entity.RegionId,
            ImportedProfessionalId = entity.ImportedProfessionalId,
            SiteKey = entity.SiteKey,
            SearchQuery = entity.SearchQuery,
            DeduplicationKey = entity.DeduplicationKey,
            Name = entity.Name,
            Phone = entity.Phone,
            WhatsApp = entity.WhatsApp,
            NormalizedPhone = entity.NormalizedPhone,
            Address = entity.Address,
            Neighborhood = entity.Neighborhood,
            City = entity.City,
            State = entity.State,
            LocalityDisplay = BuildLeadLocalityDisplay(entity.Neighborhood, entity.City, entity.State),
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Website = entity.Website,
            SourceListingUrl = entity.SourceListingUrl,
            SourceDetailsUrl = entity.SourceDetailsUrl,
            ExternalId = entity.ExternalId,
            ImportStatus = entity.ImportStatus,
            SourceSitesJson = entity.SourceSitesJson,
            SourceUrlsJson = entity.SourceUrlsJson,
            SourceCount = entity.SourceCount,
            Rating = entity.Rating,
            ReviewCount = entity.ReviewCount,
            RawPayloadJson = entity.RawPayloadJson,
            ProfessionName = entity.Profession?.Name,
            ProfessionNamesDisplay = BuildProfessionNamesDisplay(professions, entity.Profession?.Name),
            Professions = professions,
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            ImportedProfessionalName = entity.ImportedProfessional?.FullName,
            ScrapedAt = entity.ScrapedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<int> ConvertToProfessionalAsync(int leadId, UpsertProfessionalDto dto, CancellationToken cancellationToken = default)
    {
        var lead = await _context.ProviderLeads
            .FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken)
            ?? throw new NotFoundException("Lead capturado nao encontrado.");

        if (lead.ImportedProfessionalId.HasValue || string.Equals(lead.ImportStatus, "Imported", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Este lead ja foi convertido em profissional.");
        }

        var prepared = await ProfessionalWriteSupport.PrepareAsync(_context, dto, null, cancellationToken);
        var professional = ProfessionalWriteSupport.CreateEntity(dto, prepared);

        lead.ImportStatus = "Imported";
        lead.ImportedProfessional = professional;

        _context.Professionals.Add(professional);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return professional.Id;
    }

    private IQueryable<ProviderLead> BuildFilteredQuery(ProviderLeadQueryFilter filter)
    {
        var query = _context.ProviderLeads
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.SearchQuery.Contains(searchTerm) ||
                x.SiteKey.Contains(searchTerm) ||
                (x.Phone != null && x.Phone.Contains(searchTerm)) ||
                (x.WhatsApp != null && x.WhatsApp.Contains(searchTerm)) ||
                (x.Address != null && x.Address.Contains(searchTerm)));
        }

        if (filter.LeadSourceId.HasValue)
        {
            query = query.Where(x => x.LeadSourceId == filter.LeadSourceId.Value);
        }

        if (filter.ProfessionId.HasValue)
        {
            query = query.Where(x =>
                x.ProfessionId == filter.ProfessionId.Value ||
                x.ProviderLeadProfessions.Any(y => y.ProfessionId == filter.ProfessionId.Value));
        }

        if (filter.RegionId.HasValue)
        {
            query = query.Where(x => x.RegionId == filter.RegionId.Value);
        }

        if (filter.LeadCaptureRunId.HasValue)
        {
            query = query.Where(x => x.LeadCaptureRunId == filter.LeadCaptureRunId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SiteKey))
        {
            var siteKey = filter.SiteKey.Trim();
            query = query.Where(x => x.SiteKey == siteKey);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(x => x.City != null && x.City.Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(filter.ImportStatus))
        {
            var importStatus = filter.ImportStatus.Trim();
            query = query.Where(x => x.ImportStatus == importStatus);
        }

        return query;
    }

    private static ProviderLeadListItemDto MapToListItem(ProviderLead entity)
    {
        var professions = BuildProfessionLookups(entity.ProviderLeadProfessions, entity.Profession);

        return new ProviderLeadListItemDto
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            LeadSourceId = entity.LeadSourceId,
            SiteKey = entity.SiteKey,
            Name = entity.Name,
            Phone = entity.Phone,
            WhatsApp = entity.WhatsApp,
            Neighborhood = entity.Neighborhood,
            City = entity.City,
            State = entity.State,
            LocalityDisplay = BuildLeadLocalityDisplay(entity.Neighborhood, entity.City, entity.State),
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Website = entity.Website,
            ProfessionName = entity.Profession?.Name,
            ProfessionNamesDisplay = BuildProfessionNamesDisplay(professions, entity.Profession?.Name),
            Professions = professions,
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            ImportStatus = entity.ImportStatus,
            ImportedProfessionalId = entity.ImportedProfessionalId,
            ImportedProfessionalName = entity.ImportedProfessional?.FullName,
            SourceCount = entity.SourceCount,
            ScrapedAt = entity.ScrapedAt
        };
    }

    private static ProviderLeadMapItemDto MapToMapItem(ProviderLead entity)
    {
        return new ProviderLeadMapItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            LocalityDisplay = BuildLeadLocalityDisplay(entity.Neighborhood, entity.City, entity.State),
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            Latitude = entity.Latitude!.Value,
            Longitude = entity.Longitude!.Value
        };
    }

    private static ProviderLeadExportItemDto MapToExportItem(ProviderLead entity)
    {
        var professions = BuildProfessionLookups(entity.ProviderLeadProfessions, entity.Profession);

        return new ProviderLeadExportItemDto
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            LeadSourceId = entity.LeadSourceId,
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            SiteKey = entity.SiteKey,
            Name = entity.Name,
            Phone = entity.Phone,
            WhatsApp = entity.WhatsApp,
            NormalizedPhone = entity.NormalizedPhone,
            Website = entity.Website,
            SearchQuery = entity.SearchQuery,
            Address = entity.Address,
            Neighborhood = entity.Neighborhood,
            City = entity.City,
            State = entity.State,
            LocalityDisplay = BuildLeadLocalityDisplay(entity.Neighborhood, entity.City, entity.State),
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            ProfessionName = entity.Profession?.Name,
            ProfessionNamesDisplay = BuildProfessionNamesDisplay(professions, entity.Profession?.Name),
            ImportStatus = entity.ImportStatus,
            ImportedProfessionalId = entity.ImportedProfessionalId,
            ImportedProfessionalName = entity.ImportedProfessional?.FullName,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            SourceCount = entity.SourceCount,
            SourceListingUrl = entity.SourceListingUrl,
            SourceDetailsUrl = entity.SourceDetailsUrl,
            ExternalId = entity.ExternalId,
            Rating = entity.Rating,
            ReviewCount = entity.ReviewCount,
            ScrapedAt = entity.ScrapedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static IReadOnlyList<LookupItemDto> BuildProfessionLookups(
        IEnumerable<ProviderLeadProfession> relations,
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

    private static string? BuildRegionDisplay(Region? region)
    {
        if (region is null)
        {
            return null;
        }

        return string.Join(" / ",
            new[] { region.State, region.City, region.Neighborhood, region.Zone }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string BuildLeadLocalityDisplay(string? neighborhood, string? city, string? state)
    {
        return string.Join(" / ",
            new[] { neighborhood, city, state }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
