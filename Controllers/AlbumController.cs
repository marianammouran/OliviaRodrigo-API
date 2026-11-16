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
public class AlbumController : ControllerBase
{
    private readonly IAlbumRepository _albumRepository;

    public AlbumController(IAlbumRepository albumRepository)
    {
        _albumRepository = albumRepository;
    }

    /// <summary>Lista os álbuns (com suas faixas) em ordem de lançamento. Filtro opcional por nome.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busca = null)
    {
        var albuns = await _albumRepository.Listar(busca);
        return Ok(albuns.Select(AlbumRespostaDTO.Converter));
    }

    /// <summary>Busca um álbum pelo ID, com a relação de músicas.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var album = await _albumRepository.BuscarPorId(id);
        return album is null ? NotFound("Álbum não encontrado.") : Ok(AlbumRespostaDTO.Converter(album));
    }

    /// <summary>Cadastra um álbum (somente administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cadastrar([FromBody] AlbumDTO dto)
    {
        var album = Converter(dto);

        try
        {
            await _albumRepository.Cadastrar(album);
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um álbum com esse nome.");
        }
        catch (DbUpdateException erro) when (ErrosBanco.Restricao(erro))
        {
            return BadRequest("Os dados do álbum não respeitam as regras do banco.");
        }

        return CreatedAtAction(nameof(BuscarPorId), new { id = album.IdAlbum }, AlbumRespostaDTO.Converter(album));
    }

    /// <summary>Atualiza um álbum (somente administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AlbumDTO dto)
    {
        if (await _albumRepository.BuscarPorId(id) is null) return NotFound("Álbum não encontrado.");

        try
        {
            await _albumRepository.Atualizar(id, Converter(dto));
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe um álbum com esse nome.");
        }
        catch (DbUpdateException erro) when (ErrosBanco.Restricao(erro))
        {
            return BadRequest("Os dados do álbum não respeitam as regras do banco.");
        }

        return NoContent();
    }

    /// <summary>Exclui um álbum sem músicas (somente administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await _albumRepository.BuscarPorId(id) is null) return NotFound("Álbum não encontrado.");

        if (await _albumRepository.PossuiMusicas(id))
            return Conflict("Este álbum possui músicas cadastradas. Exclua ou mova as músicas antes de excluí-lo.");

        await _albumRepository.Excluir(id);
        return NoContent();
    }

    private static Album Converter(AlbumDTO dto) => new()
    {
        Nome = dto.Nome.Trim(),
        DataLancamento = dto.DataLancamento!.Value,
        Capa = string.IsNullOrWhiteSpace(dto.Capa) ? null : dto.Capa.Trim(),
        Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim(),
        TipoLancamento = dto.TipoLancamento
    };
}
