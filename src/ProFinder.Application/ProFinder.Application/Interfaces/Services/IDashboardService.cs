using ProFinder.Application.DTOs.Dashboard;

namespace ProFinder.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
