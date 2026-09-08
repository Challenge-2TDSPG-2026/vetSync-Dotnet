using Microsoft.AspNetCore.Mvc;
using VetApi.DTOs;
using VetApi.Services;

namespace VetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Clínicas")]
public class ClinicasController : ControllerBase
{
    private const double RaioPadraoKm = 10.0;
    private const double RaioMaximoKm = 50.0;

    private readonly IVeterinaryClinicSearchService _searchService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ILogger<ClinicasController> _logger;

    public ClinicasController(
        IVeterinaryClinicSearchService searchService,
        IGeoLocationService geoLocationService,
        ILogger<ClinicasController> logger)
    {
        _searchService = searchService;
        _geoLocationService = geoLocationService;
        _logger = logger;
    }

    [HttpGet("proximas")]
    [ProducesResponseType(typeof(IEnumerable<ClinicaVeterinariaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProximas(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double raioKm = RaioPadraoKm,
        CancellationToken cancellationToken = default)
    {
        if (!_geoLocationService.CoordenadasValidas(latitude, longitude))
        {
            _logger.LogWarning("Coordenadas inválidas recebidas: Lat={Latitude} Lon={Longitude}", latitude, longitude);
            return BadRequest(new { message = "Latitude deve estar entre -90 e 90 e longitude entre -180 e 180." });
        }

        if (raioKm <= 0 || raioKm > RaioMaximoKm)
        {
            return BadRequest(new { message = $"O raio de busca deve ser maior que zero e no máximo {RaioMaximoKm}km." });
        }

        _logger.LogInformation(
            "Buscando clínicas veterinárias reais num raio de {RaioKm}km a partir de ({Latitude}, {Longitude})",
            raioKm, latitude, longitude);

        try
        {
            var encontradas = await _searchService.BuscarProximasAsync(latitude, longitude, raioKm, cancellationToken);

            var resposta = encontradas.Select(c => new ClinicaVeterinariaDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Endereco = c.Endereco,
                Telefone = c.Telefone,
                SiteOuRedeSocial = c.SiteOuRedeSocial,
                Latitude = c.Latitude,
                Longitude = c.Longitude,
                DistanciaKm = c.DistanciaKm,
                Fonte = c.Fonte,
                LinkMapa = c.LinkMapa
            });

            return Ok(resposta);
        }
        catch (VeterinaryClinicSearchException ex)
        {
            _logger.LogError(ex, "Falha ao consultar a fonte externa de clínicas veterinárias");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Não foi possível buscar clínicas reais no momento. Tente novamente em instantes." });
        }
    }
}
