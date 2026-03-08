using System.ComponentModel.DataAnnotations;

namespace ProFinder.Application.DTOs.Professions;

public class ProfessionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public class UpsertProfessionDto
{
    [Required(ErrorMessage = "Informe o nome da profissão.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;
}
