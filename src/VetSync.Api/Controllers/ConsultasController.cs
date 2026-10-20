using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;

namespace VetSync.Api.Controllers;

[Tags("Consultas")]
public class ConsultasController : ApiControllerBase
{
    private readonly IConsultaService _service;

    public ConsultasController(IConsultaService service) => _service = service;

    /// <summary>Lista consultas com filtros (petId, status, de, ate), ordenação e paginação</summary>
    /// <remarks>Ordenação: sortBy = id | dataConsulta | status | veterinario; sortDir = asc | desc.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ConsultaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] ConsultaQuery query, CancellationToken ct)
        => Ok((await _service.GetPagedAsync(query, ct)).WithLinks(Request));

    /// <summary>Busca consulta por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ConsultaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok((await _service.GetByIdAsync(id, ct)).WithLinks(Request));

    /// <summary>Agenda uma nova consulta</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConsultaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateConsultaDto dto, CancellationToken ct)
    {
        var criada = (await _service.CreateAsync(dto, ct)).WithLinks(Request);
        return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza dados/status de uma consulta</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConsultaDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Remove uma consulta (somente perfil Admin)</summary>
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
