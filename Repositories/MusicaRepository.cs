using DiarioRoxo.WebAPI.Context;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Repositories;

public class MusicaRepository : IMusicaRepository
{
    private readonly DiarioRoxoDbContext _context;

    public MusicaRepository(DiarioRoxoDbContext context)
    {
        _context = context;
    }

    public async Task Atualizar(int id, Musica musica)
    {
        var musicaExistente = await _context.Musica.FindAsync(id);
        if (musicaExistente != null)
        {
            musicaExistente.Titulo = musica.Titulo;
            musicaExistente.IdAlbum = musica.IdAlbum;
            musicaExistente.IdGenero = musica.IdGenero;
            musicaExistente.NumeroFaixa = musica.NumeroFaixa;
            musicaExistente.DataLancamento = musica.DataLancamento;
            musicaExistente.Duracao = musica.Duracao;
            musicaExistente.ImagemCapa = musica.ImagemCapa;
            musicaExistente.Descricao = musica.Descricao;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Musica?> BuscarPorId(int id)
    {
        return await _context.Musica.AsNoTracking()
            .Include(m => m.IdAlbumNavigation)
            .Include(m => m.IdGeneroNavigation)
            .FirstOrDefaultAsync(m => m.IdMusica == id);
    }

    public async Task Cadastrar(Musica musica)
    {
        await _context.Musica.AddAsync(musica);
        await _context.SaveChangesAsync();
    }

    public async Task Excluir(int id)
    {
        var musicaBuscada = await _context.Musica.FindAsync(id);
        if (musicaBuscada != null)
        {
            _context.Musica.Remove(musicaBuscada);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(List<Musica> musicas, int total)> ListarPaginado(
        int pagina = 1,
        int tamanhoPagina = 12,
        string? titulo = null,
        string? album = null,
        string? busca = null,
        int? idAlbum = null,
        int? idGenero = null)
    {
        var query = _context.Musica.AsNoTracking()
            .Include(m => m.IdAlbumNavigation)
            .Include(m => m.IdGeneroNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(m => m.Titulo.Contains(titulo));

        if (!string.IsNullOrWhiteSpace(album))
            query = query.Where(m => m.IdAlbumNavigation.Nome.Contains(album));

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Titulo.Contains(busca) || m.IdAlbumNavigation.Nome.Contains(busca));

        if (idAlbum is not null)
            query = query.Where(m => m.IdAlbum == idAlbum);

        if (idGenero is not null)
            query = query.Where(m => m.IdGenero == idGenero);

        int total = await query.CountAsync();

        var itens = await query
            .OrderBy(m => m.IdAlbumNavigation.DataLancamento)
            .ThenBy(m => m.IdAlbum)
            .ThenBy(m => m.NumeroFaixa)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}
