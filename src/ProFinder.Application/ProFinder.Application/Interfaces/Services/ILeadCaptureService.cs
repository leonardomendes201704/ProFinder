using ProFinder.Application.DTOs.Capture;

namespace ProFinder.Application.Interfaces.Services;

public interface ILeadCaptureService
{
    Task<LeadCaptureRunDto> StartCaptureAsync(StartLeadCaptureDto dto, CancellationToken cancellationToken = default);

    Task<LeadCaptureRunDto> ImportCsvAsync(ImportCsvRequestDto dto, CancellationToken cancellationToken = default);
}
