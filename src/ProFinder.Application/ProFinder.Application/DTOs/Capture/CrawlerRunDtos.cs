using System.ComponentModel.DataAnnotations;

namespace ProFinder.Application.DTOs.Capture;

public class CrawlerRunListItemDto
{
    public int Id { get; set; }

    public string CaptureType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? SearchQuery { get; set; }

    public string? Notes { get; set; }

    public string? ErrorMessage { get; set; }

    public string? CreatedBy { get; set; }

    public int ItemsCaptured { get; set; }

    public int ItemsInserted { get; set; }

    public int ItemsUpdated { get; set; }

    public int ItemsSkipped { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}

public class CrawlerRunDetailsDto : CrawlerRunListItemDto
{
    public int? LeadSourceId { get; set; }

    public string? SourceUrl { get; set; }

    public string? FileName { get; set; }

    public int ProviderLeadCount { get; set; }

    public bool CanStop { get; set; }
}

public class CrawlerRunLogDto
{
    public int Id { get; set; }

    public int LeadCaptureRunId { get; set; }

    public string LogLevel { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CrawlerDataResetResultDto
{
    public int LeadCaptureRunCount { get; set; }

    public int ProviderLeadCount { get; set; }

    public int GoogleMapsLeadCount { get; set; }

    public int LeadCaptureLogCount { get; set; }

    public string RequestedBy { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}

public class StartCrawlerRunRequestDto
{
    [Required(ErrorMessage = "Informe a cidade.")]
    [StringLength(120)]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o servico/profissao alvo.")]
    [StringLength(120)]
    [Display(Name = "Servico")]
    public string Service { get; set; } = string.Empty;

    [Display(Name = "Profissao")]
    public int? ProfessionId { get; set; }

    [Display(Name = "Regiao")]
    public int? RegionId { get; set; }

    [Display(Name = "Sites selecionados")]
    public List<string> SelectedSites { get; set; } = [];

    [Display(Name = "Modo do navegador")]
    public string? HeadlessMode { get; set; }

    [Range(1, 50000, ErrorMessage = "Informe um valor entre 1 e 50000.")]
    [Display(Name = "Maximo de registros")]
    public int? MaxRecords { get; set; }

    [StringLength(100)]
    [Display(Name = "Solicitado por")]
    public string? RequestedBy { get; set; }
}

public class CrawlerLauncherDefaultsDto
{
    public string PythonExecutable { get; set; } = string.Empty;

    public string ScriptPath { get; set; } = string.Empty;

    public string WorkingDirectory { get; set; } = string.Empty;

    public string ExportDirectory { get; set; } = string.Empty;

    public string DefaultSitesCsv { get; set; } = string.Empty;

    public string LogLevel { get; set; } = string.Empty;

    public string SqlDriver { get; set; } = string.Empty;

    public bool HasConnectionStringOverride { get; set; }
}
