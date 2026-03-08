using ProFinder.Application.DTOs.Interactions;

namespace ProFinder.Application.Interfaces.Services;

public interface IInteractionService
{
    Task<IReadOnlyList<InteractionDto>> GetListAsync(int? professionalId = null, CancellationToken cancellationToken = default);

    Task<InteractionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(UpsertInteractionDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, UpsertInteractionDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
