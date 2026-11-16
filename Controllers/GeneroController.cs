using DiarioRoxo.WebAPI.DTO;
using DiarioRoxo.WebAPI.Interfaces;
using DiarioRoxo.WebAPI.Models;
using DiarioRoxo.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeneroController : ControllerBase
{
    private readonly IGeneroRepository _generoRepository;

    public GeneroController(IGeneroRepository generoRepository)
    {
        _generoRepository = generoRepository;
    }

    /// <summary>Lista todos os gêneros em ordem alfabética.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await _generoRepository.Listar());

    /// <summary>Busca um gênero pelo ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var genero = await _generoRepository.BuscarPorId(id);
        return genero is null ? NotFound("Gênero não encontrado.") : Ok(genero);
    }

    /// <summary>Cadastra um gênero (somente administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cadastrar([FromBody] GeneroDTO dto)
    {
        var genero = new Genero { Nome = dto.Nome.Trim() };

        try
        {
            await _generoRepository.Cadastrar(genero);
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um gênero com esse nome.");
        }

        return CreatedAtAction(nameof(BuscarPorId), new { id = genero.IdGenero }, genero);
    }

    /// <summary>Atualiza um gênero (somente administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] GeneroDTO dto)
    {
        if (await _generoRepository.BuscarPorId(id) is null) return NotFound("Gênero não encontrado.");

        try
        {
            await _generoRepository.Atualizar(id, new Genero { Nome = dto.Nome.Trim() });
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um gênero com esse nome.");
        }

        return NoContent();
    }

    /// <summary>Exclui um gênero sem músicas associadas (somente administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await _generoRepository.BuscarPorId(id) is null) return NotFound("Gênero não encontrado.");

        if (await _generoRepository.PossuiMusicas(id))
            return Conflict("Este gênero possui músicas associadas. Altere o gênero dessas músicas antes de excluí-lo.");

        await _generoRepository.Excluir(id);
        return NoContent();
    }
}
