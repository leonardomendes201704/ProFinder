using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.DTOs.ProviderLeads;
using ProFinder.Application.Filters;

namespace ProFinder.Application.Interfaces.Services;

public interface IProviderLeadService
{
    Task<PagedResult<ProviderLeadListItemDto>> GetPagedAsync(ProviderLeadQueryFilter filter, CancellationToken cancellationToken = default);

    Task<ProviderLeadDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> ConvertToProfessionalAsync(int leadId, UpsertProfessionalDto dto, CancellationToken cancellationToken = default);
}
