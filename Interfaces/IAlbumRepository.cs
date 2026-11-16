using DiarioRoxo.WebAPI.Models;

namespace DiarioRoxo.WebAPI.Interfaces;

public interface IAlbumRepository
{
    Task Cadastrar(Album album);
    Task Atualizar(int id, Album album);
    Task Excluir(int id);
    Task<Album?> BuscarPorId(int id);
    Task<List<Album>> Listar(string? busca = null);
    Task<bool> PossuiMusicas(int id);
}
