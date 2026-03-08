using ProFinder.Application.Common;
using ProFinder.Application.DTOs.GoogleMapsLeads;
using ProFinder.Application.Filters;

namespace ProFinder.Application.Interfaces.Services;

public interface IGoogleMapsLeadService
{
    Task<PagedResult<GoogleMapsLeadListItemDto>> GetPagedAsync(GoogleMapsLeadQueryFilter filter, CancellationToken cancellationToken = default);

    Task<GoogleMapsLeadDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
