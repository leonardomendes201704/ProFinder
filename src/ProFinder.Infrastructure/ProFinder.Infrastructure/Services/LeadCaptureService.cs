using Microsoft.Extensions.Logging;
using ProFinder.Application.DTOs.Capture;
using ProFinder.Application.Interfaces.Services;
using ProFinder.Domain.Entities;
using ProFinder.Infrastructure.Data;

namespace ProFinder.Infrastructure.Services;

public class LeadCaptureService : ILeadCaptureService
{
    private readonly ProFinderDbContext _context;
    private readonly CsvLeadImporterService _csvLeadImporterService;
    private readonly ILogger<LeadCaptureService> _logger;

    public LeadCaptureService(
        ProFinderDbContext context,
        CsvLeadImporterService csvLeadImporterService,
        ILogger<LeadCaptureService> logger)
    {
        _context = context;
        _csvLeadImporterService = csvLeadImporterService;
        _logger = logger;
    }

    public async Task<LeadCaptureRunDto> StartCaptureAsync(StartLeadCaptureDto dto, CancellationToken cancellationToken = default)
    {
        var run = new LeadCaptureRun
        {
            LeadSourceId = dto.LeadSourceId,
            CaptureType = dto.CaptureType.Trim(),
            Status = "PendingImplementation",
            SourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl) ? null : dto.SourceUrl.Trim(),
            SearchQuery = string.IsNullOrWhiteSpace(dto.SearchQuery) ? null : dto.SearchQuery.Trim(),
            Notes = "Placeholder para futura integração com scraping e conectores externos.",
            CreatedBy = string.IsNullOrWhiteSpace(dto.RequestedBy) ? "System" : dto.RequestedBy.Trim(),
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _context.LeadCaptureRuns.Add(run);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Lead capture placeholder started. RunId: {RunId}", run.Id);

        return new LeadCaptureRunDto
        {
            Id = run.Id,
            LeadSourceId = run.LeadSourceId,
            CaptureType = run.CaptureType,
            Status = run.Status,
            SourceUrl = run.SourceUrl,
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

    public Task<LeadCaptureRunDto> ImportCsvAsync(ImportCsvRequestDto dto, CancellationToken cancellationToken = default)
    {
        // O importador real pode validar colunas, mapear duplicidades e criar evidências por linha.
        return _csvLeadImporterService.ImportAsync(dto, cancellationToken);
    }
}
