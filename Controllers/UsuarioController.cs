using DiarioRoxo.WebAPI.DTO;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using DiarioRoxo.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DiarioRoxo.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>Lista todos os usuários (somente administrador).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _usuarioRepository.Listar();
        return Ok(usuarios.Select(UsuarioRespostaDTO.Converter));
    }

    /// <summary>Retorna o perfil do usuário autenticado.</summary>
    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        if (IdUsuarioLogado() is not { } id) return Unauthorized();
        var usuario = await _usuarioRepository.BuscarPorId(id);
        return usuario is null ? NotFound("Usuário não encontrado.") : Ok(UsuarioRespostaDTO.Converter(usuario));
    }

    /// <summary>Busca um usuário pelo ID (o próprio usuário ou um administrador).</summary>
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        if (!PodeAcessar(id)) return Forbid();
        var usuario = await _usuarioRepository.BuscarPorId(id);
        return usuario is null ? NotFound("Usuário não encontrado.") : Ok(UsuarioRespostaDTO.Converter(usuario));
    }

    /// <summary>
    /// Cadastra um usuário. Aberto para registro público (sempre perfil "Usuario").
    /// Um administrador autenticado pode criar outro administrador informando TipoUsuario = "Admin".
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Cadastrar([FromBody] UsuarioDTO dto)
    {
        var ehAdmin = User.IsInRole("Admin");
        if (dto.TipoUsuario == "Admin" && !ehAdmin)
            return Forbid();

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Senha = dto.Senha,
            TipoUsuario = ehAdmin && dto.TipoUsuario == "Admin" ? "Admin" : "Usuario",
            Ativo = true
        };

        try
        {
            await _usuarioRepository.Cadastrar(usuario);
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um usuário com esse e-mail.");
        }

        return CreatedAtAction(nameof(BuscarPorId), new { id = usuario.IdUsuario }, UsuarioRespostaDTO.Converter(usuario));
    }

    /// <summary>
    /// Atualiza um usuário. O próprio usuário pode editar nome, e-mail e senha.
    /// Somente o administrador pode alterar TipoUsuario e Ativo.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UsuarioAtualizacaoDTO dto)
    {
        if (!PodeAcessar(id)) return Forbid();
        var atual = await _usuarioRepository.BuscarPorId(id);
        if (atual is null) return NotFound("Usuário não encontrado.");

        var ehAdmin = User.IsInRole("Admin");
        var novoTipo = dto.TipoUsuario ?? atual.TipoUsuario;
        var novoAtivo = dto.Ativo ?? atual.Ativo;

        if (!ehAdmin && (novoTipo != atual.TipoUsuario || novoAtivo != atual.Ativo))
            return Forbid();

        if (ehAdmin && IdUsuarioLogado() == id && (novoTipo != "Admin" || !novoAtivo))
            return BadRequest("Você não pode remover o próprio acesso de administrador nem desativar a própria conta.");

        try
        {
            await _usuarioRepository.Atualizar(id, new Usuario
            {
                Nome = dto.Nome.Trim(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                Senha = dto.Senha ?? string.Empty,
                TipoUsuario = novoTipo,
                Ativo = novoAtivo
            });
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um usuário com esse e-mail.");
        }

        return NoContent();
    }

    /// <summary>Desativa um usuário (soft delete pelo trigger do banco). Somente administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (IdUsuarioLogado() == id) return BadRequest("Você não pode desativar a própria conta.");
        if (await _usuarioRepository.BuscarPorId(id) is null) return NotFound("Usuário não encontrado.");
        await _usuarioRepository.Excluir(id);
        return NoContent();
    }

    private int? IdUsuarioLogado() =>
        int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    private bool PodeAcessar(int id) => User.IsInRole("Admin") || IdUsuarioLogado() == id;
}
