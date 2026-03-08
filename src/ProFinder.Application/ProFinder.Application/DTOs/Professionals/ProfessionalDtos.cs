using System.ComponentModel.DataAnnotations;
using ProFinder.Application.Common;
using ProFinder.Application.DTOs.Interactions;

namespace ProFinder.Application.DTOs.Professionals;

public class ProfessionalListItemDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? BusinessName { get; set; }

    public string? Phone { get; set; }

    public string? WhatsApp { get; set; }

    public string? Email { get; set; }

    public string ProfessionName { get; set; } = string.Empty;

    public string ProfessionNamesDisplay { get; set; } = string.Empty;

    public IReadOnlyList<LookupItemDto> Professions { get; set; } = [];

    public string StatusName { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string? PrimaryRegionDisplay { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class ProfessionalDetailsDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? BusinessName { get; set; }

    public string? Phone { get; set; }

    public string? WhatsApp { get; set; }

    public string? Email { get; set; }

    public string? DocumentNumber { get; set; }

    public int ProfessionId { get; set; }

    public string ProfessionName { get; set; } = string.Empty;

    public string ProfessionNamesDisplay { get; set; } = string.Empty;

    public IReadOnlyList<LookupItemDto> Professions { get; set; } = [];

    public int SourceId { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public int StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? Website { get; set; }

    public string? Instagram { get; set; }

    public bool IsAutonomous { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public IReadOnlyList<ProfessionalRegionDto> Regions { get; set; } = [];

    public IReadOnlyList<InteractionDto> Interactions { get; set; } = [];

    public IReadOnlyList<EvidenceDto> Evidences { get; set; } = [];
}

public class ProfessionalRegionDto
{
    public int RegionId { get; set; }

    public string RegionDisplayName { get; set; } = string.Empty;

    public string? ConfidenceLevel { get; set; }

    public bool IsPrimaryRegion { get; set; }
}

public class EvidenceDto
{
    public int Id { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string FieldValue { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public DateTime CollectedAt { get; set; }
}

public class UpsertProfessionalDto : IValidatableObject
{
    [Required(ErrorMessage = "Informe o nome do profissional.")]
    [StringLength(200, ErrorMessage = "O nome deve ter no maximo 200 caracteres.")]
    [Display(Name = "Nome completo")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "O nome comercial deve ter no maximo 200 caracteres.")]
    [Display(Name = "Nome comercial")]
    public string? BusinessName { get; set; }

    [StringLength(20, ErrorMessage = "O telefone deve ter no maximo 20 caracteres.")]
    [Display(Name = "Telefone")]
    public string? Phone { get; set; }

    [StringLength(20, ErrorMessage = "O WhatsApp deve ter no maximo 20 caracteres.")]
    [Display(Name = "WhatsApp")]
    public string? WhatsApp { get; set; }

    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no maximo 150 caracteres.")]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(30, ErrorMessage = "O documento deve ter no maximo 30 caracteres.")]
    [Display(Name = "Documento")]
    public string? DocumentNumber { get; set; }

    [Required(ErrorMessage = "Selecione a profissao principal.")]
    [Display(Name = "Profissao principal")]
    public int ProfessionId { get; set; }

    [Display(Name = "Profissoes")]
    public List<int> ProfessionIds { get; set; } = [];

    [Required(ErrorMessage = "Selecione a origem do lead.")]
    [Display(Name = "Origem")]
    public int SourceId { get; set; }

    [Required(ErrorMessage = "Selecione o status do lead.")]
    [Display(Name = "Status")]
    public int StatusId { get; set; }

    [StringLength(2000, ErrorMessage = "As observacoes devem ter no maximo 2000 caracteres.")]
    [Display(Name = "Observacoes")]
    public string? Notes { get; set; }

    [Url(ErrorMessage = "Informe uma URL valida.")]
    [StringLength(200, ErrorMessage = "O website deve ter no maximo 200 caracteres.")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [StringLength(100, ErrorMessage = "O Instagram deve ter no maximo 100 caracteres.")]
    [Display(Name = "Instagram")]
    public string? Instagram { get; set; }

    [Display(Name = "Profissional autonomo")]
    public bool IsAutonomous { get; set; } = true;

    [Display(Name = "Ativo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Regioes de atuacao")]
    public List<int> RegionIds { get; set; } = [];

    [Display(Name = "Regiao principal")]
    public int? PrimaryRegionId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Phone) &&
            string.IsNullOrWhiteSpace(WhatsApp) &&
            string.IsNullOrWhiteSpace(Email))
        {
            yield return new ValidationResult(
                "Informe ao menos um meio de contato: telefone, WhatsApp ou e-mail.",
                [nameof(Phone), nameof(WhatsApp), nameof(Email)]);
        }

        if (PrimaryRegionId.HasValue && !RegionIds.Contains(PrimaryRegionId.Value))
        {
            yield return new ValidationResult(
                "A regiao principal precisa estar entre as regioes selecionadas.",
                [nameof(PrimaryRegionId), nameof(RegionIds)]);
        }

        if (ProfessionId <= 0)
        {
            yield return new ValidationResult(
                "Selecione a profissao principal.",
                [nameof(ProfessionId)]);
        }

        if (ProfessionIds.Count > 0 && !ProfessionIds.Contains(ProfessionId))
        {
            yield return new ValidationResult(
                "A profissao principal precisa estar entre as profissoes selecionadas.",
                [nameof(ProfessionId), nameof(ProfessionIds)]);
        }
    }
}
