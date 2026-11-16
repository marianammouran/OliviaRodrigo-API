using DiarioRoxo.WebAPI.Models;

namespace DiarioRoxo.WebAPI.DTO;

public record UsuarioRespostaDTO(int IdUsuario, string Nome, string Email, string TipoUsuario, bool Ativo, DateTime DataCadastro)
{
    public static UsuarioRespostaDTO Converter(Usuario usuario) => new(usuario.IdUsuario,
        usuario.Nome,
        usuario.Email,
        usuario.TipoUsuario,
        usuario.Ativo,
        usuario.DataCadastro);
}
