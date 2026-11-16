using DiarioRoxo.WebAPI.Context;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Repositories;

public class GeneroRepository : IGeneroRepository
{
    private readonly DiarioRoxoDbContext _context;

    public GeneroRepository(DiarioRoxoDbContext context)
    {
        _context = context;
    }

    public async Task Atualizar(int id, Genero genero)
    {
        var generoExistente = await _context.Genero.FindAsync(id);
        if (generoExistente != null)
        {
            generoExistente.Nome = genero.Nome;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Genero?> BuscarPorId(int id)
    {
        return await _context.Genero.AsNoTracking().FirstOrDefaultAsync(g => g.IdGenero == id);
    }

    public async Task Cadastrar(Genero genero)
    {
        await _context.Genero.AddAsync(genero);
        await _context.SaveChangesAsync();
    }

    public async Task Excluir(int id)
    {
        var generoBuscado = await _context.Genero.FindAsync(id);
        if (generoBuscado != null)
        {
            _context.Genero.Remove(generoBuscado);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Genero>> Listar()
    {
        return await _context.Genero.AsNoTracking().OrderBy(g => g.Nome).ToListAsync();
    }

    public async Task<bool> PossuiMusicas(int id)
    {
        return await _context.Musica.AnyAsync(m => m.IdGenero == id);
    }
}
