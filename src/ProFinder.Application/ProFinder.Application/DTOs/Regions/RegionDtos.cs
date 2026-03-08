using System.ComponentModel.DataAnnotations;

namespace ProFinder.Application.DTOs.Regions;

public class RegionDto
{
    public int Id { get; set; }

    public string State { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Neighborhood { get; set; }

    public string? ZipCode { get; set; }

    public string? Zone { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? RadiusKm { get; set; }

    public bool IsActive { get; set; }

    public string DisplayName =>
        string.Join(" / ", new[] { State, City, Neighborhood, Zone }.Where(value => !string.IsNullOrWhiteSpace(value)));
}

public class UpsertRegionDto
{
    [Required(ErrorMessage = "Informe o estado.")]
    [StringLength(2, ErrorMessage = "Use a sigla do estado.")]
    [Display(Name = "Estado")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade.")]
    [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
    [Display(Name = "Bairro")]
    public string? Neighborhood { get; set; }

    [StringLength(20, ErrorMessage = "O CEP deve ter no máximo 20 caracteres.")]
    [Display(Name = "CEP")]
    public string? ZipCode { get; set; }

    [StringLength(50, ErrorMessage = "A zona deve ter no máximo 50 caracteres.")]
    [Display(Name = "Zona")]
    public string? Zone { get; set; }

    [Display(Name = "Latitude")]
    public decimal? Latitude { get; set; }

    [Display(Name = "Longitude")]
    public decimal? Longitude { get; set; }

    [Display(Name = "Raio (km)")]
    public decimal? RadiusKm { get; set; }

    [Display(Name = "Ativa")]
    public bool IsActive { get; set; } = true;
}
