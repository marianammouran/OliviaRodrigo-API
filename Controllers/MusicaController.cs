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
public class MusicaController : ControllerBase
{
    private readonly IMusicaRepository _musicaRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly IGeneroRepository _generoRepository;

    public MusicaController(IMusicaRepository musicaRepository, IAlbumRepository albumRepository, IGeneroRepository generoRepository)
    {
        _musicaRepository = musicaRepository;
        _albumRepository = albumRepository;
        _generoRepository = generoRepository;
    }

    /// <summary>
    /// Lista as músicas com paginação, pesquisa e filtros.
    /// </summary>
    /// <param name="pagina">Página (a partir de 1).</param>
    /// <param name="tamanhoPagina">Itens por página (1 a 100).</param>
    /// <param name="titulo">Pesquisa pelo título da música.</param>
    /// <param name="album">Pesquisa pelo nome do álbum.</param>
    /// <param name="busca">Pesquisa no título OU no nome do álbum.</param>
    /// <param name="idAlbum">Filtra por álbum.</param>
    /// <param name="idGenero">Filtra por gênero.</param>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 12,
        [FromQuery] string? titulo = null,
        [FromQuery] string? album = null,
        [FromQuery] string? busca = null,
        [FromQuery] int? idAlbum = null,
        [FromQuery] int? idGenero = null)
    {
        if (pagina < 1 || tamanhoPagina is < 1 or > 100)
            return BadRequest("Página deve ser positiva e o tamanhoPagina deve estar entre 1 e 100.");

        var (items, total) = await _musicaRepository.ListarPaginado(pagina, tamanhoPagina, titulo, album, busca, idAlbum, idGenero);
        return Ok(new { items, total, pagina, tamanhoPagina });
    }

    /// <summary>Busca uma música pelo ID (com álbum e gênero).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var musica = await _musicaRepository.BuscarPorId(id);
        return musica is null ? NotFound("Música não encontrada.") : Ok(musica);
    }

    /// <summary>Cadastra uma música (somente administrador).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cadastrar([FromBody] MusicaDTO dto)
    {
        var erroRelacao = await ValidarRelacionamentos(dto);
        if (erroRelacao is not null) return BadRequest(erroRelacao);

        var musica = Converter(dto);

        try
        {
            await _musicaRepository.Cadastrar(musica);
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe uma música com esse título ou com esse número de faixa neste álbum.");
        }
        catch (DbUpdateException erro) when (ErrosBanco.Restricao(erro))
        {
            return BadRequest("Os dados da música não respeitam as regras do banco.");
        }

        var criada = await _musicaRepository.BuscarPorId(musica.IdMusica);
        return CreatedAtAction(nameof(BuscarPorId), new { id = musica.IdMusica }, criada);
    }

    /// <summary>Atualiza uma música (somente administrador).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] MusicaDTO dto)
    {
        if (await _musicaRepository.BuscarPorId(id) is null) return NotFound("Música não encontrada.");

        var erroRelacao = await ValidarRelacionamentos(dto);
        if (erroRelacao is not null) return BadRequest(erroRelacao);

        try
        {
            await _musicaRepository.Atualizar(id, Converter(dto));
        }
        catch (DbUpdateException erro) when (ErrosBanco.Duplicado(erro))
        {
            return Conflict("Já existe uma música com esse título ou com esse número de faixa neste álbum.");
        }
        catch (DbUpdateException erro) when (ErrosBanco.Restricao(erro))
        {
            return BadRequest("Os dados da música não respeitam as regras do banco.");
        }

        return NoContent();
    }

    /// <summary>Exclui uma música (somente administrador).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        if (await _musicaRepository.BuscarPorId(id) is null) return NotFound("Música não encontrada.");
        await _musicaRepository.Excluir(id);
        return NoContent();
    }

    private async Task<string?> ValidarRelacionamentos(MusicaDTO dto)
    {
        if (await _albumRepository.BuscarPorId(dto.IdAlbum) is null) return "O álbum informado não existe.";
        if (await _generoRepository.BuscarPorId(dto.IdGenero) is null) return "O gênero informado não existe.";
        return null;
    }

    private static Musica Converter(MusicaDTO dto) => new()
    {
        Titulo = dto.Titulo.Trim(),
        IdAlbum = dto.IdAlbum,
        IdGenero = dto.IdGenero,
        NumeroFaixa = dto.NumeroFaixa,
        DataLancamento = dto.DataLancamento!.Value,
        Duracao = dto.Duracao,
        ImagemCapa = string.IsNullOrWhiteSpace(dto.ImagemCapa) ? null : dto.ImagemCapa.Trim(),
        Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim()
    };
}
