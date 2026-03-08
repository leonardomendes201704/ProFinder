using Microsoft.Extensions.Logging;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class CsvLeadImporterService
{
    private readonly ProFinderDbContext _context;
    private readonly ILogger<CsvLeadImporterService> _logger;

    public CsvLeadImporterService(ProFinderDbContext context, ILogger<CsvLeadImporterService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LeadCaptureRunDto> ImportAsync(ImportCsvRequestDto dto, CancellationToken cancellationToken = default)
    {
        var run = new LeadCaptureRun
        {
            LeadSourceId = dto.LeadSourceId,
            CaptureType = "CsvImport",
            Status = "PendingImplementation",
            FileName = dto.FileName.Trim(),
            Notes = "Placeholder para futura rotina de importação e consolidação de leads via CSV.",
            CreatedBy = string.IsNullOrWhiteSpace(dto.RequestedBy) ? "System" : dto.RequestedBy.Trim(),
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _context.LeadCaptureRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CSV import placeholder started. RunId: {RunId}", run.Id);

        return new LeadCaptureRunDto
        {
            Id = run.Id,
            LeadSourceId = run.LeadSourceId,
            CaptureType = run.CaptureType,
            Status = run.Status,
            FileName = run.FileName,
            SearchQuery = run.SearchQuery,
            Notes = run.Notes,
            ErrorMessage = run.ErrorMessage,
            ItemsCaptured = run.ItemsCaptured,
            ItemsInserted = run.ItemsInserted,
            ItemsUpdated = run.ItemsUpdated,
            ItemsSkipped = run.ItemsSkipped,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt
        };
    }
}
