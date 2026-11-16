using DiarioRoxo.WebAPI.Models;

namespace DiarioRoxo.WebAPI.DTO;

public record FaixaResumoDTO(int IdMusica, int NumeroFaixa, string Titulo, int Duracao, int IdGenero, string? Genero);

public record AlbumRespostaDTO(
    int IdAlbum,
    string Nome,
    DateOnly DataLancamento,
    string? Capa,
    string? Descricao,
    string TipoLancamento,
    int TotalMusicas,
    List<FaixaResumoDTO> Musicas)
{
    public static AlbumRespostaDTO Converter(Album album) => new(
        album.IdAlbum,
        album.Nome,
        album.DataLancamento,
        album.Capa,
        album.Descricao,
        album.TipoLancamento,
        album.Musica.Count,
        album.Musica
            .OrderBy(m => m.NumeroFaixa)
            .Select(m => new FaixaResumoDTO(m.IdMusica, m.NumeroFaixa, m.Titulo, m.Duracao, m.IdGenero, m.IdGeneroNavigation?.Nome))
            .ToList());
}
