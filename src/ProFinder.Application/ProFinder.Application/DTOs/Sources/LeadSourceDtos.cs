using System.ComponentModel.DataAnnotations;

namespace ProFinder.Application.DTOs.Sources;

public class LeadSourceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public bool IsActive { get; set; }
}

public class UpsertLeadSourceDto
{
    [Required(ErrorMessage = "Informe o nome da origem.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Informe uma URL válida.")]
    [StringLength(200, ErrorMessage = "A URL deve ter no máximo 200 caracteres.")]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;
}
