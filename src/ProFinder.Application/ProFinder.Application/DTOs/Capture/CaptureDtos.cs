using System.ComponentModel.DataAnnotations;

namespace ProFinder.Application.DTOs.Capture;

public class LeadCaptureRunDto
{
    public int Id { get; set; }

    public int? LeadSourceId { get; set; }

    public string CaptureType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public string? FileName { get; set; }

    public string? SearchQuery { get; set; }

    public string? Notes { get; set; }

    public string? ErrorMessage { get; set; }

    public int ItemsCaptured { get; set; }

    public int ItemsInserted { get; set; }

    public int ItemsUpdated { get; set; }

    public int ItemsSkipped { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}

public class StartLeadCaptureDto
{
    [Display(Name = "Origem")]
    public int? LeadSourceId { get; set; }

    [Required(ErrorMessage = "Informe o tipo de captura.")]
    [StringLength(50)]
    [Display(Name = "Tipo de captura")]
    public string CaptureType { get; set; } = "Scraping";

    [Url(ErrorMessage = "Informe uma URL válida.")]
    [Display(Name = "URL de origem")]
    public string? SourceUrl { get; set; }

    [StringLength(200)]
    [Display(Name = "Consulta de busca")]
    public string? SearchQuery { get; set; }

    [StringLength(100)]
    [Display(Name = "Solicitado por")]
    public string? RequestedBy { get; set; }
}

public class ImportCsvRequestDto
{
    [Display(Name = "Origem")]
    public int? LeadSourceId { get; set; }

    [Required(ErrorMessage = "Informe o nome do arquivo.")]
    [StringLength(200)]
    [Display(Name = "Arquivo CSV")]
    public string FileName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Solicitado por")]
    public string? RequestedBy { get; set; }
}
