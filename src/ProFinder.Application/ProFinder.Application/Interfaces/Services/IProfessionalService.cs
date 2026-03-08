using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Application.Filters;

namespace ProFinder.Application.Interfaces.Services;

public interface IProfessionalService
{
    Task<PagedResult<ProfessionalListItemDto>> GetPagedAsync(ProfessionalQueryFilter filter, CancellationToken cancellationToken = default);

    Task<ProfessionalDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);

    Task<int> CreateAsync(UpsertProfessionalDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpsertProfessionalDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
