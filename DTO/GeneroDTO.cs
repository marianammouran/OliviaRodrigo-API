using System.ComponentModel.DataAnnotations;

namespace DiarioRoxo.WebAPI.DTO;

public class GeneroDTO
{
    [Required(ErrorMessage = "O nome do gênero é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome do gênero deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}
