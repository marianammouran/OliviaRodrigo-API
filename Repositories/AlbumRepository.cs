using DiarioRoxo.WebAPI.Context;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Repositories;

public class AlbumRepository : IAlbumRepository
{
    private readonly DiarioRoxoDbContext _context;

    public AlbumRepository(DiarioRoxoDbContext context)
    {
        _context = context;
    }

    public async Task Atualizar(int id, Album album)
    {
        var albumExistente = await _context.Album.FindAsync(id);
        if (albumExistente != null)
        {
            albumExistente.Nome = album.Nome;
            albumExistente.DataLancamento = album.DataLancamento;
            albumExistente.Capa = album.Capa;
            albumExistente.Descricao = album.Descricao;
            albumExistente.TipoLancamento = album.TipoLancamento;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Album?> BuscarPorId(int id)
    {
        return await _context.Album.AsNoTracking()
            .Include(a => a.Musica).ThenInclude(m => m.IdGeneroNavigation)
            .FirstOrDefaultAsync(a => a.IdAlbum == id);
    }

    public async Task Cadastrar(Album album)
    {
        await _context.Album.AddAsync(album);
        await _context.SaveChangesAsync();
    }

    public async Task Excluir(int id)
    {
        var albumBuscado = await _context.Album.FindAsync(id);
        if (albumBuscado != null)
        {
            _context.Album.Remove(albumBuscado);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Album>> Listar(string? busca = null)
    {
        var query = _context.Album.AsNoTracking()
            .Include(a => a.Musica).ThenInclude(m => m.IdGeneroNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(a => a.Nome.Contains(busca));

        return await query.OrderBy(a => a.DataLancamento).ThenBy(a => a.Nome).ToListAsync();
    }

    public async Task<bool> PossuiMusicas(int id)
    {
        return await _context.Musica.AnyAsync(m => m.IdAlbum == id);
    }
}
