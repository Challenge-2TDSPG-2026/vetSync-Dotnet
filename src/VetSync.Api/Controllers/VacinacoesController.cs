using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;

namespace VetSync.Api.Controllers;

[Tags("Vacinações")]
public class VacinacoesController : ApiControllerBase
{
    private readonly IVacinacaoService _service;

    public VacinacoesController(IVacinacaoService service) => _service = service;

    /// <summary>Lista vacinações com filtros (petId, proximaDoseAte), ordenação e paginação</summary>
    /// <remarks>Ordenação: sortBy = id | dataAplicacao | proximaDose | vacina; sortDir = asc | desc.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VacinacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll([FromQuery] VacinacaoQuery query, CancellationToken ct)
        => Ok((await _service.GetPagedAsync(query, ct)).WithLinks(Request));

    /// <summary>Busca vacinação por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VacinacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => Ok((await _service.GetByIdAsync(id, ct)).WithLinks(Request));

    /// <summary>Registra uma vacina aplicada</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VacinacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateVacinacaoDto dto, CancellationToken ct)
    {
        var criada = (await _service.CreateAsync(dto, ct)).WithLinks(Request);
        return CreatedAtAction(nameof(GetById), new { id = criada.Id }, criada);
    }

    /// <summary>Remove registro de vacinação (somente perfil Admin)</summary>
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
