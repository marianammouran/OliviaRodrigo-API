using System.ComponentModel.DataAnnotations;

namespace DiarioRoxo.WebAPI.DTO;

public class UsuarioDTO
{
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    /// <summary>Somente o administrador pode informar "Admin". No cadastro público é sempre "Usuario".</summary>
    [RegularExpression("^(Admin|Usuario)$", ErrorMessage = "TipoUsuario deve ser Admin ou Usuario.")]
    public string? TipoUsuario { get; set; }
}
