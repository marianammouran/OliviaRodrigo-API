using System.ComponentModel.DataAnnotations;

namespace DiarioRoxo.WebAPI.DTO;

public class MusicaDTO : IValidatableObject
{
    [Required(ErrorMessage = "O título da música é obrigatório.")]
    [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Informe um álbum válido.")]
    public int IdAlbum { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um gênero válido.")]
    public int IdGenero { get; set; }

    [Range(1, 999, ErrorMessage = "O número da faixa deve estar entre 1 e 999.")]
    public int NumeroFaixa { get; set; }

    [Required(ErrorMessage = "A data de lançamento é obrigatória.")]
    public DateOnly? DataLancamento { get; set; }

    /// <summary>Duração em segundos.</summary>
    [Range(1, 36000, ErrorMessage = "A duração (em segundos) deve ser positiva.")]
    public int Duracao { get; set; }

    [StringLength(500, ErrorMessage = "A URL da imagem deve ter no máximo 500 caracteres.")]
    public string? ImagemCapa { get; set; }

    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataLancamento is { } data && (data.Year < 1900 || data > DateOnly.FromDateTime(DateTime.Today).AddYears(1)))
            yield return new ValidationResult(
                "A data de lançamento deve estar entre 1900 e o próximo ano.",
                new[] { nameof(DataLancamento) });
    }
}
