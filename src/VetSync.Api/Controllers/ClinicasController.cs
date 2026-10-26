using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Application.DTOs;
using VetSync.Application.Services;

namespace VetSync.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Clínicas")]
public class ClinicasController : ControllerBase
{
    private readonly IClinicaService _clinicaService;

    public ClinicasController(IClinicaService clinicaService) => _clinicaService = clinicaService;

    /// <summary>Busca clínicas veterinárias reais próximas de uma coordenada (OpenStreetMap)</summary>
    [HttpGet("proximas")]
    [ProducesResponseType(typeof(IEnumerable<ClinicaVeterinariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProximas(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = ClinicaService.RaioPadraoKm,
        CancellationToken cancellationToken = default)
        => Ok(await _clinicaService.BuscarProximasAsync(latitude, longitude, raioKm, cancellationToken));
}
