using System.ComponentModel.DataAnnotations;
using ProFinder.Domain.Enums;

namespace ProFinder.Application.DTOs.Interactions;

public class InteractionDto
{
    public int Id { get; set; }

    public int ProfessionalId { get; set; }

    public string ProfessionalName { get; set; } = string.Empty;

    public InteractionType InteractionType { get; set; }

    public string InteractionTypeLabel { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime InteractionDate { get; set; }

    public string? CreatedBy { get; set; }
}

public class UpsertInteractionDto
{
    [Required(ErrorMessage = "Selecione o profissional.")]
    [Display(Name = "Profissional")]
    public int ProfessionalId { get; set; }

    [Required(ErrorMessage = "Selecione o tipo de interação.")]
    [Display(Name = "Tipo de interação")]
    public InteractionType InteractionType { get; set; }

    [Required(ErrorMessage = "Informe a descrição da interação.")]
    [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Data da interação")]
    public DateTime InteractionDate { get; set; } = DateTime.Now;

    [StringLength(100, ErrorMessage = "O campo Criado por deve ter no máximo 100 caracteres.")]
    [Display(Name = "Criado por")]
    public string? CreatedBy { get; set; } = "Admin";
}
