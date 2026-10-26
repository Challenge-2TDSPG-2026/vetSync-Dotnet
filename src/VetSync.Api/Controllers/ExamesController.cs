using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;

namespace VetSync.Api.Controllers;

[Tags("Exames")]
public class ExamesController : ApiControllerBase
{
    private readonly IExameService _service;

    public ExamesController(IExameService service) => _service = service;

    /// <summary>Lista exames com filtros (petId, tipo), ordenação e paginação</summary>
    /// <remarks>Ordenação: sortBy = id | dataExame | tipoExame; sortDir = asc | desc.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ExameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] ExameQuery query, CancellationToken ct)
        => Ok((await _service.GetPagedAsync(query, ct)).WithLinks(Request));

    /// <summary>Busca exame por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ExameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok((await _service.GetByIdAsync(id, ct)).WithLinks(Request));

    /// <summary>Registra um exame</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExameDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateExameDto dto, CancellationToken ct)
    {
        var criado = (await _service.CreateAsync(dto, ct)).WithLinks(Request);
        return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um exame (ex.: lançar resultado)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExameDto dto, CancellationToken ct)
    {
        await _service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>Remove um exame (somente perfil Admin)</summary>
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
