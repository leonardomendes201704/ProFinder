using Microsoft.EntityFrameworkCore;
using ProFinder.Application.Common;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.GoogleMapsLeads;
using ProFinder.Application.Filters;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class GoogleMapsLeadService : IGoogleMapsLeadService
{
    private readonly ProFinderDbContext _context;

    public GoogleMapsLeadService(ProFinderDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GoogleMapsLeadListItemDto>> GetPagedAsync(GoogleMapsLeadQueryFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.GoogleMapsLeads
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.Region)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim();
            query = query.Where(x =>
                x.Name.Contains(searchTerm) ||
                x.SearchQuery.Contains(searchTerm) ||
                (x.Phone != null && x.Phone.Contains(searchTerm)) ||
                (x.Address != null && x.Address.Contains(searchTerm)));
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

        return new PagedResult<GoogleMapsLeadListItemDto>
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount,
            Items = items.Select(MapToListItem).ToList()
        };
    }

    public async Task<GoogleMapsLeadDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.GoogleMapsLeads
            .AsNoTracking()
            .Include(x => x.Profession)
            .Include(x => x.Region)
            .Include(x => x.LeadSource)
            .Include(x => x.ImportedProfessional)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Lead do Google Maps não encontrado.");

        return new GoogleMapsLeadDetailsDto
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            LeadSourceId = entity.LeadSourceId,
            ProfessionId = entity.ProfessionId,
            RegionId = entity.RegionId,
            ImportedProfessionalId = entity.ImportedProfessionalId,
            Name = entity.Name,
            SearchQuery = entity.SearchQuery,
            PlaceUrl = entity.PlaceUrl,
            Phone = entity.Phone,
            NormalizedPhone = entity.NormalizedPhone,
            Address = entity.Address,
            Website = entity.Website,
            Rating = entity.Rating,
            ReviewCount = entity.ReviewCount,
            ImportStatus = entity.ImportStatus,
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

    private static GoogleMapsLeadListItemDto MapToListItem(GoogleMapsLead entity) =>
        new()
        {
            Id = entity.Id,
            LeadCaptureRunId = entity.LeadCaptureRunId,
            Name = entity.Name,
            Phone = entity.Phone,
            Address = entity.Address,
            Website = entity.Website,
            Rating = entity.Rating,
            ReviewCount = entity.ReviewCount,
            SearchQuery = entity.SearchQuery,
            ImportStatus = entity.ImportStatus,
            ProfessionName = entity.Profession?.Name,
            RegionDisplayName = BuildRegionDisplay(entity.Region),
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
