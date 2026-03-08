using ProFinder.Application.DTOs.Professions;
using ProFinder.Application.DTOs.Regions;
using ProFinder.Application.DTOs.Sources;
using ProFinder.Application.DTOs.Statuses;

namespace ProFinder.Application.Interfaces.Services;

public interface ILookupService
{
    Task<IReadOnlyList<ProfessionDto>> GetProfessionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<ProfessionDto> GetProfessionByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateProfessionAsync(UpsertProfessionDto dto, CancellationToken cancellationToken = default);

    Task UpdateProfessionAsync(int id, UpsertProfessionDto dto, CancellationToken cancellationToken = default);

    Task DeleteProfessionAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegionDto>> GetRegionsAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<RegionDto> GetRegionByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateRegionAsync(UpsertRegionDto dto, CancellationToken cancellationToken = default);

    Task UpdateRegionAsync(int id, UpsertRegionDto dto, CancellationToken cancellationToken = default);

    Task DeleteRegionAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeadSourceDto>> GetSourcesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<LeadSourceDto> GetSourceByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateSourceAsync(UpsertLeadSourceDto dto, CancellationToken cancellationToken = default);

    Task UpdateSourceAsync(int id, UpsertLeadSourceDto dto, CancellationToken cancellationToken = default);

    Task DeleteSourceAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeadStatusDto>> GetStatusesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

    Task<LeadStatusDto> GetStatusByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> CreateStatusAsync(UpsertLeadStatusDto dto, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(int id, UpsertLeadStatusDto dto, CancellationToken cancellationToken = default);

    Task DeleteStatusAsync(int id, CancellationToken cancellationToken = default);
}
