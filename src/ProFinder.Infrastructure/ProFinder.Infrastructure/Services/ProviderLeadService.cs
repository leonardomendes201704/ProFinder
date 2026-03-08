using Microsoft.EntityFrameworkCore;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
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
        var query = _context.ProviderLeads
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .AsSplitQuery()
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
            query = query.Where(x => x.ProfessionId == filter.ProfessionId.Value);
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

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
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

    public async Task<ProviderLeadDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ProviderLeads
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .Include(x => x.ImportedProfessional)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Lead capturado nao encontrado.");

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
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            ImportedProfessionalName = entity.ImportedProfessional?.FullName,
            ScrapedAt = entity.ScrapedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static ProviderLeadListItemDto MapToListItem(ProviderLead entity) =>
        new()
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            LeadSourceId = entity.LeadSourceId,
            SiteKey = entity.SiteKey,
            Name = entity.Name,
            Phone = entity.Phone,
            WhatsApp = entity.WhatsApp,
            City = entity.City,
            State = entity.State,
            Website = entity.Website,
            ProfessionName = entity.Profession?.Name,
            RegionDisplayName = BuildRegionDisplay(entity.Region),
            SourceName = entity.LeadSource?.Name ?? string.Empty,
            ImportStatus = entity.ImportStatus,
            SourceCount = entity.SourceCount,
            ScrapedAt = entity.ScrapedAt
        };

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
}
