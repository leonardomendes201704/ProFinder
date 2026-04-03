using Microsoft.EntityFrameworkCore;
using ProFinder.Application.Common.Exceptions;
using ProFinder.Application.DTOs.Professionals;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;
using ProFinder.Infrastructure.Helpers;

namespace ProFinder.Infrastructure.Services;

internal static class ProfessionalWriteSupport
{
    internal sealed record PreparedProfessionalWrite(
        IReadOnlyList<int> ProfessionIds,
        string FullName,
        string? BusinessName,
        string? Phone,
        string? WhatsApp,
        string? Email,
        string? DocumentNumber,
        string? Notes,
        string? Website,
        string? Instagram);

    public static async Task<PreparedProfessionalWrite> PrepareAsync(
        ProFinderDbContext context,
        UpsertProfessionalDto dto,
        int? currentId,
        CancellationToken cancellationToken)
    {
        var professionIds = GetEffectiveProfessionIds(dto);

        await EnsureLookupReferencesAsync(context, professionIds, dto.SourceId, dto.StatusId, dto.RegionIds, cancellationToken);
        var normalized = NormalizeProfessional(dto);
        await EnsureProfessionalIsUniqueAsync(context, normalized.Phone, normalized.WhatsApp, normalized.Email, currentId, cancellationToken);

        return new PreparedProfessionalWrite(
            professionIds,
            normalized.FullName,
            normalized.BusinessName,
            normalized.Phone,
            normalized.WhatsApp,
            normalized.Email,
            normalized.DocumentNumber,
            normalized.Notes,
            normalized.Website,
            normalized.Instagram);
    }

    public static Professional CreateEntity(UpsertProfessionalDto dto, PreparedProfessionalWrite prepared)
    {
        var professional = new Professional
        {
            FullName = prepared.FullName,
            BusinessName = prepared.BusinessName,
            Phone = prepared.Phone,
            WhatsApp = prepared.WhatsApp,
            Email = prepared.Email,
            DocumentNumber = prepared.DocumentNumber,
            ProfessionId = dto.ProfessionId,
            SourceId = dto.SourceId,
            StatusId = dto.StatusId,
            Notes = prepared.Notes,
            Website = prepared.Website,
            Instagram = prepared.Instagram,
            IsAutonomous = dto.IsAutonomous,
            IsActive = dto.IsActive
        };

        ApplyProfessions(professional, prepared.ProfessionIds, dto.ProfessionId);
        ApplyRegions(professional, dto.RegionIds, dto.PrimaryRegionId);

        return professional;
    }

    public static void ApplyToExisting(
        Professional professional,
        UpsertProfessionalDto dto,
        PreparedProfessionalWrite prepared,
        ProFinderDbContext context)
    {
        professional.FullName = prepared.FullName;
        professional.BusinessName = prepared.BusinessName;
        professional.Phone = prepared.Phone;
        professional.WhatsApp = prepared.WhatsApp;
        professional.Email = prepared.Email;
        professional.DocumentNumber = prepared.DocumentNumber;
        professional.ProfessionId = dto.ProfessionId;
        professional.SourceId = dto.SourceId;
        professional.StatusId = dto.StatusId;
        professional.Notes = prepared.Notes;
        professional.Website = prepared.Website;
        professional.Instagram = prepared.Instagram;
        professional.IsAutonomous = dto.IsAutonomous;
        professional.IsActive = dto.IsActive;

        context.ProfessionalRegions.RemoveRange(professional.ProfessionalRegions);
        professional.ProfessionalRegions.Clear();
        ApplyRegions(professional, dto.RegionIds, dto.PrimaryRegionId);

        context.ProfessionalProfessions.RemoveRange(professional.ProfessionalProfessions);
        professional.ProfessionalProfessions.Clear();
        ApplyProfessions(professional, prepared.ProfessionIds, dto.ProfessionId);
    }

    private static async Task EnsureLookupReferencesAsync(
        ProFinderDbContext context,
        IReadOnlyCollection<int> professionIds,
        int sourceId,
        int statusId,
        IEnumerable<int> regionIds,
        CancellationToken cancellationToken)
    {
        if (professionIds.Count == 0)
        {
            throw new ValidationException("Selecione ao menos uma profissao.");
        }

        var existingProfessionCount = await context.Professions.CountAsync(
            x => professionIds.Contains(x.Id) && x.IsActive,
            cancellationToken);
        var sourceExists = await context.LeadSources.AnyAsync(x => x.Id == sourceId && x.IsActive, cancellationToken);
        var statusExists = await context.LeadStatuses.AnyAsync(x => x.Id == statusId && x.IsActive, cancellationToken);

        if (existingProfessionCount != professionIds.Count || !sourceExists || !statusExists)
        {
            throw new ValidationException("Profissao, origem ou status invalido.");
        }

        var distinctRegionIds = regionIds.Distinct().ToList();
        if (distinctRegionIds.Count == 0)
        {
            return;
        }

        var existingCount = await context.Regions.CountAsync(
            x => distinctRegionIds.Contains(x.Id) && x.IsActive,
            cancellationToken);

        if (existingCount != distinctRegionIds.Count)
        {
            throw new ValidationException("Uma ou mais regioes informadas nao existem ou estao inativas.");
        }
    }

    private static async Task EnsureProfessionalIsUniqueAsync(
        ProFinderDbContext context,
        string? phone,
        string? whatsApp,
        string? email,
        int? currentId,
        CancellationToken cancellationToken)
    {
        var exists = await context.Professionals.AnyAsync(
            x => x.Id != currentId &&
                 x.IsActive &&
                 (
                     (phone != null && (x.Phone == phone || x.WhatsApp == phone)) ||
                     (whatsApp != null && (x.Phone == whatsApp || x.WhatsApp == whatsApp)) ||
                     (email != null && x.Email == email)
                 ),
            cancellationToken);

        if (exists)
        {
            throw new DuplicateResourceException("Ja existe um profissional ativo com o mesmo telefone, WhatsApp ou e-mail.");
        }
    }

    private static (string FullName, string? BusinessName, string? Phone, string? WhatsApp, string? Email, string? DocumentNumber, string? Notes, string? Website, string? Instagram) NormalizeProfessional(UpsertProfessionalDto dto)
    {
        var fullName = dto.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ValidationException("O nome do profissional e obrigatorio.");
        }

        return
        (
            fullName,
            NormalizeOptional(dto.BusinessName),
            PhoneNormalizer.Normalize(dto.Phone),
            PhoneNormalizer.Normalize(dto.WhatsApp),
            NormalizeEmail(dto.Email),
            NormalizeOptional(dto.DocumentNumber),
            NormalizeOptional(dto.Notes),
            NormalizeOptional(dto.Website),
            NormalizeOptional(dto.Instagram)
        );
    }

    private static IReadOnlyList<int> GetEffectiveProfessionIds(UpsertProfessionalDto dto)
    {
        return dto.ProfessionIds
            .Append(dto.ProfessionId)
            .Where(x => x > 0)
            .Distinct()
            .ToList();
    }

    private static void ApplyProfessions(Professional professional, IReadOnlyCollection<int> professionIds, int primaryProfessionId)
    {
        foreach (var professionId in professionIds.Distinct())
        {
            professional.ProfessionalProfessions.Add(new ProfessionalProfession
            {
                ProfessionId = professionId,
                IsPrimary = professionId == primaryProfessionId
            });
        }
    }

    private static void ApplyRegions(Professional professional, IReadOnlyCollection<int> regionIds, int? primaryRegionId)
    {
        var distinctRegionIds = regionIds.Distinct().ToList();

        if (distinctRegionIds.Count == 0)
        {
            return;
        }

        var effectivePrimaryRegionId = primaryRegionId ?? distinctRegionIds.First();

        foreach (var regionId in distinctRegionIds)
        {
            professional.ProfessionalRegions.Add(new ProfessionalRegion
            {
                RegionId = regionId,
                ConfidenceLevel = regionId == effectivePrimaryRegionId ? "Alta" : "Media",
                IsPrimaryRegion = regionId == effectivePrimaryRegionId
            });
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
