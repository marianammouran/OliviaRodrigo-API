using System.ComponentModel.DataAnnotations;

namespace DiarioRoxo.WebAPI.DTO;

public class UsuarioAtualizacaoDTO
{
    [Required(ErrorMessage = "O nome é obrigatório."), StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório."), EmailAddress(ErrorMessage = "E-mail inválido."), StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Opcional: preencha apenas para trocar a senha.</summary>
    [MinLength(8, ErrorMessage = "A nova senha deve ter pelo menos 8 caracteres.")]
    public string? Senha { get; set; }

    /// <summary>Somente administrador.</summary>
    [RegularExpression("^(Admin|Usuario)$", ErrorMessage = "TipoUsuario deve ser Admin ou Usuario.")]
    public string? TipoUsuario { get; set; }

    /// <summary>Somente administrador (reativar/desativar).</summary>
    public bool? Ativo { get; set; }
}
