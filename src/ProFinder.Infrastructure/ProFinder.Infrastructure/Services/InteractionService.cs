using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Interactions;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Helpers;

namespace ProFinder.Infrastructure.Services;

public class InteractionService : IInteractionService
{
    private readonly ProFinderDbContext _context;
    private readonly ILogger<InteractionService> _logger;

    public InteractionService(ProFinderDbContext context, ILogger<InteractionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InteractionDto>> GetListAsync(int? professionalId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Interactions
            .AsNoTracking()
            .Include(x => x.Professional)
            .AsQueryable();

        if (professionalId.HasValue)
        {
            query = query.Where(x => x.ProfessionalId == professionalId.Value);
        }

        var interactions = await query
            .OrderByDescending(x => x.InteractionDate)
            .ToListAsync(cancellationToken);

        return interactions.Select(MapToDto).ToList();
    }

    public async Task<InteractionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Interactions
            .AsNoTracking()
            .Include(x => x.Professional)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Interação não encontrada.");

        return MapToDto(entity);
    }

    public async Task<int> CreateAsync(UpsertInteractionDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureProfessionalExistsAsync(dto.ProfessionalId, cancellationToken);

        var entity = new Interaction
        {
            ProfessionalId = dto.ProfessionalId,
            InteractionType = dto.InteractionType,
            Description = dto.Description.Trim(),
            InteractionDate = dto.InteractionDate,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "Admin" : dto.CreatedBy.Trim()
        };

        _context.Interactions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Interaction {InteractionId} created for professional {ProfessionalId}.", entity.Id, entity.ProfessionalId);
        return entity.Id;
    }

    public async Task UpdateAsync(int id, UpsertInteractionDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureProfessionalExistsAsync(dto.ProfessionalId, cancellationToken);

        var entity = await _context.Interactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Interação não encontrada.");

        entity.ProfessionalId = dto.ProfessionalId;
        entity.InteractionType = dto.InteractionType;
        entity.Description = dto.Description.Trim();
        entity.InteractionDate = dto.InteractionDate;
        entity.CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "Admin" : dto.CreatedBy.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Interaction {InteractionId} updated.", entity.Id);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Interactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Interação não encontrada.");

        _context.Interactions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Interaction {InteractionId} deleted.", entity.Id);
    }

    private async Task EnsureProfessionalExistsAsync(int professionalId, CancellationToken cancellationToken)
    {
        var exists = await _context.Professionals.AnyAsync(x => x.Id == professionalId, cancellationToken);

        if (!exists)
        {
            throw new ValidationException("O profissional informado não existe.");
        }
    }

    private static InteractionDto MapToDto(Interaction entity) =>
        new()
        {
            Id = entity.Id,
            ProfessionalId = entity.ProfessionalId,
            ProfessionalName = entity.Professional?.FullName ?? string.Empty,
            InteractionType = entity.InteractionType,
            InteractionTypeLabel = entity.InteractionType.ToDisplayLabel(),
            Description = entity.Description,
            InteractionDate = entity.InteractionDate,
            CreatedBy = entity.CreatedBy
        };
}
