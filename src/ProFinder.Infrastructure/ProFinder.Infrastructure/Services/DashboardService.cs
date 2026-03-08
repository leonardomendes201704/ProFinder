using Microsoft.EntityFrameworkCore;
using ProFinder.Application.DTOs.Dashboard;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ProFinderDbContext _context;

    public DashboardService(ProFinderDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalProfessionals = await _context.Professionals
            .AsNoTracking()
            .CountAsync(x => x.IsActive, cancellationToken);

        var byProfession = await _context.Professionals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .GroupBy(x => x.Profession!.Name)
            .Select(group => new DashboardMetricItemDto
            {
                Label = group.Key,
                Total = group.Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var byStatus = await _context.Professionals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .GroupBy(x => x.Status!.Name)
            .Select(group => new DashboardMetricItemDto
            {
                Label = group.Key,
                Total = group.Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var byCity = await _context.ProfessionalRegions
            .AsNoTracking()
            .Where(x => x.Professional!.IsActive && x.Region!.IsActive)
            .GroupBy(x => x.Region!.City)
            .Select(group => new DashboardMetricItemDto
            {
                Label = group.Key,
                Total = group.Select(x => x.ProfessionalId).Distinct().Count()
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var recent = await _context.Professionals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.Profession)
            .Include(x => x.Status)
            .Include(x => x.ProfessionalRegions)
                .ThenInclude(x => x.Region)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalProfessionals = totalProfessionals,
            ProfessionalsByProfession = byProfession,
            ProfessionalsByStatus = byStatus,
            ProfessionalsByCity = byCity,
            RecentProfessionals = recent.Select(x => new RecentProfessionalDto
            {
                Id = x.Id,
                FullName = x.FullName,
                ProfessionName = x.Profession?.Name ?? string.Empty,
                StatusName = x.Status?.Name ?? string.Empty,
                City = x.ProfessionalRegions
                    .OrderByDescending(region => region.IsPrimaryRegion)
                    .Select(region => region.Region?.City)
                    .FirstOrDefault(city => !string.IsNullOrWhiteSpace(city)),
                CreatedAt = x.CreatedAt
            }).ToList()
        };
    }
}
