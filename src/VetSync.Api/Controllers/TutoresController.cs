using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;

namespace VetSync.Api.Controllers;

[Tags("Tutores")]
public class TutoresController : ApiControllerBase
{
    private readonly ITutorService _service;

    public TutoresController(ITutorService service) => _service = service;

    /// <summary>Lista tutores com filtros, ordenação e paginação</summary>
    /// <remarks>Ordenação: sortBy = id | nome | email | criadoEm; sortDir = asc | desc.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TutorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] TutorQuery query, CancellationToken ct)
        => Ok((await _service.GetPagedAsync(query, ct)).WithLinks(Request));

    /// <summary>Busca tutor por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok((await _service.GetByIdAsync(id, ct)).WithLinks(Request));

    /// <summary>Cadastra um novo tutor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTutorDto dto, CancellationToken ct)
    {
        var criado = (await _service.CreateAsync(dto, ct)).WithLinks(Request);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza dados de um tutor</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTutorDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Remove um tutor (somente perfil Admin)</summary>
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
