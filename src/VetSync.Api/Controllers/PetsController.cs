using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;

namespace VetSync.Api.Controllers;

[Tags("Pets")]
public class PetsController : ApiControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service) => _service = service;

    /// <summary>Lista pets com filtros, ordenação e paginação</summary>
    /// <remarks>Ordenação: sortBy = id | nome | especie | raca | criadoEm; sortDir = asc | desc.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] PetQuery query, CancellationToken ct)
        => Ok((await _service.GetPagedAsync(query, ct)).WithLinks(Request));

    /// <summary>Busca pet por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok((await _service.GetByIdAsync(id, ct)).WithLinks(Request));

    /// <summary>Retorna a jornada completa do pet (consultas, vacinas, exames) do banco relacional</summary>
    [HttpGet("{id:int}/jornada")]
    [ProducesResponseType(typeof(JornadaPetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJornada(int id, CancellationToken ct)
    {
        var jornada = await _service.GetJornadaAsync(id, ct);
        jornada.Pet.WithLinks(Request);
        jornada.Tutor.WithLinks(Request);
        jornada.Consultas.ForEach(c => c.WithLinks(Request));
        jornada.Vacinacoes.ForEach(v => v.WithLinks(Request));
        jornada.Exames.ForEach(e => e.WithLinks(Request));
        return Ok(jornada);
    }

    /// <summary>Linha do tempo de eventos do pet (MongoDB), do mais recente ao mais antigo</summary>
    [HttpGet("{id:int}/timeline")]
    [ProducesResponseType(typeof(IEnumerable<JornadaEventoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimeline(int id, [FromQuery] int limite = 50, CancellationToken ct = default)
        => Ok(await _service.GetTimelineAsync(id, limite, ct));

    /// <summary>Cadastra um novo pet</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto, CancellationToken ct)
    {
        var criado = (await _service.CreateAsync(dto, ct)).WithLinks(Request);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza dados de um pet</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Remove um pet (somente perfil Admin)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
