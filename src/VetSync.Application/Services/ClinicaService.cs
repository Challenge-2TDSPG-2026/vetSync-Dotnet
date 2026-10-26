using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Application.DTOs;
using VetSync.Domain.Exceptions;

namespace VetSync.Application.Services;

public class ClinicaService : IClinicaService
{
    public const double RaioPadraoKm = 10.0;
    public const double RaioMaximoKm = 50.0;

    private readonly IVeterinaryClinicSearchService _searchService;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ILogger<ClinicaService> _logger;

    public ClinicaService(IVeterinaryClinicSearchService searchService, IGeoLocationService geoLocationService,
        ILogger<ClinicaService> logger)
    {
        _searchService = searchService;
        _geoLocationService = geoLocationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClinicaVeterinariaDto>> BuscarProximasAsync(
        double latitude, double longitude, double raioKm, CancellationToken ct = default)
    {
        if (!_geoLocationService.CoordenadasValidas(latitude, longitude))
        {
            _logger.LogWarning("Coordenadas inválidas recebidas: Lat={Latitude} Lon={Longitude}", latitude, longitude);
            throw new BusinessRuleException("Latitude deve estar entre -90 e 90 e longitude entre -180 e 180.");
        }

        if (raioKm <= 0 || raioKm > RaioMaximoKm)
            throw new BusinessRuleException($"O raio de busca deve ser maior que zero e no máximo {RaioMaximoKm}km.");

        _logger.LogInformation(
            "Buscando clínicas veterinárias num raio de {RaioKm}km a partir de ({Latitude}, {Longitude})",
            raioKm, latitude, longitude);

        var encontradas = await _searchService.BuscarProximasAsync(latitude, longitude, raioKm, ct);

        return encontradas.Select(c => new ClinicaVeterinariaDto
        {
            Id = c.Id, Nome = c.Nome, Endereco = c.Endereco, Telefone = c.Telefone,
            SiteOuRedeSocial = c.SiteOuRedeSocial, Latitude = c.Latitude, Longitude = c.Longitude,
            DistanciaKm = c.DistanciaKm, Fonte = c.Fonte, LinkMapa = c.LinkMapa
        }).ToList();
    }
}
