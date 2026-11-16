using System.ComponentModel.DataAnnotations;

namespace DiarioRoxo.WebAPI.DTO;

public class AlbumDTO : IValidatableObject
{
    public static readonly string[] TiposPermitidos = { "Album", "EP", "Single", "Deluxe", "Outro" };

    [Required(ErrorMessage = "O nome do álbum é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data de lançamento é obrigatória.")]
    public DateOnly? DataLancamento { get; set; }

    [StringLength(500, ErrorMessage = "A URL da capa deve ter no máximo 500 caracteres.")]
    public string? Capa { get; set; }

    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "O tipo de lançamento é obrigatório.")]
    public string TipoLancamento { get; set; } = "Album";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TiposPermitidos.Contains(TipoLancamento))
            yield return new ValidationResult(
                $"Tipo de lançamento inválido. Use: {string.Join(", ", TiposPermitidos)}.",
                new[] { nameof(TipoLancamento) });

        if (DataLancamento is { } data && (data.Year < 1900 || data > DateOnly.FromDateTime(DateTime.Today).AddYears(1)))
            yield return new ValidationResult(
                "A data de lançamento deve estar entre 1900 e o próximo ano.",
                new[] { nameof(DataLancamento) });
    }
}
