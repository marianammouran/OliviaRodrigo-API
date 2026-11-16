using DiarioRoxo.WebAPI.Models;

namespace DiarioRoxo.WebAPI.Interfaces;

public interface IGeneroRepository
{
    Task Cadastrar(Genero genero);
    Task Atualizar(int id, Genero genero);
    Task Excluir(int id);
    Task<Genero?> BuscarPorId(int id);
    Task<List<Genero>> Listar();
    Task<bool> PossuiMusicas(int id);
}
