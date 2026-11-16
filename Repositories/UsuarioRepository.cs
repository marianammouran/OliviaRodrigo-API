using DiarioRoxo.WebAPI.Context;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using DiarioRoxo.WebAPI.Utils;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly DiarioRoxoDbContext _context;

    public UsuarioRepository(DiarioRoxoDbContext context)
    {
        _context = context;
    }

    public async Task Atualizar(int id, Usuario usuario)
    {
        var usuarioExistente = await _context.Usuario.FindAsync(id);
        if (usuarioExistente != null)
        {
            usuarioExistente.Nome = usuario.Nome;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.TipoUsuario = usuario.TipoUsuario;
            usuarioExistente.Ativo = usuario.Ativo;
            if (!string.IsNullOrEmpty(usuario.Senha))
            {
                usuarioExistente.Senha = Criptografia.GerarHash(usuario.Senha);
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Usuario?> BuscarPorEmailESenha(string email, string senha)
    {
        var usuarioBuscado = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        if (usuarioBuscado == null)
        {
            return null;
        }

        bool senhaValida = Criptografia.CompararHash(senha, usuarioBuscado.Senha);

        if (!senhaValida) { return null; }

        return usuarioBuscado;
    }

    public async Task<Usuario?> BuscarPorId(int id)
    {
        return await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
    }

    public async Task Cadastrar(Usuario usuario)
    {
        usuario.Senha = Criptografia.GerarHash(usuario.Senha);

        await _context.Usuario.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task Excluir(int id)
    {
        // O trigger TR_UsuarioSoftDelete transforma o DELETE em Ativo = 0.
        var usuarioBuscado = await _context.Usuario.FindAsync(id);
        if (usuarioBuscado != null)
        {
            _context.Usuario.Remove(usuarioBuscado);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Usuario>> Listar()
    {
        return await _context.Usuario.AsNoTracking().OrderBy(u => u.Nome).ToListAsync();
    }
}
