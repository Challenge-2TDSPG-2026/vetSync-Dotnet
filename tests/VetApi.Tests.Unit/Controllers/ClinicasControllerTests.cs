using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VetApi.Controllers;
using VetApi.DTOs;
using VetApi.Services;
using Xunit;

namespace VetApi.Tests.Unit.Controllers;

public class ClinicasControllerTests
{
    private readonly Mock<IVeterinaryClinicSearchService> _searchServiceMock = new();
    private readonly Mock<ILogger<ClinicasController>> _loggerMock = new();
    private readonly IGeoLocationService _geoLocationService = new GeoLocationService();

    private ClinicasController CriarController() =>
        new(_searchServiceMock.Object, _geoLocationService, _loggerMock.Object);

    [Fact]
    public async Task GetProximas_CoordenadasInvalidas_RetornaBadRequestSemChamarServicoExterno()
    {
        var controller = CriarController();

        var resultado = await controller.GetProximas(latitude: 200, longitude: 200);

        resultado.Should().BeOfType<BadRequestObjectResult>();
        _searchServiceMock.Verify(
            s => s.BuscarProximasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(51)]
    public async Task GetProximas_RaioForaDoIntervaloPermitido_RetornaBadRequest(double raioKm)
    {
        var controller = CriarController();

        var resultado = await controller.GetProximas(latitude: -23.5, longitude: -46.6, raioKm: raioKm);

        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetProximas_ParametrosValidos_RetornaOkComClinicasDoServico()
    {
        var clinicasEncontradas = new List<ClinicaVeterinariaEncontrada>
        {
            new()
            {
                Id = "node/1",
                Nome = "Clínica Real Um",
                Latitude = -23.5,
                Longitude = -46.6,
                DistanciaKm = 1.2
            }
        };

        _searchServiceMock
            .Setup(s => s.BuscarProximasAsync(-23.5, -46.6, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clinicasEncontradas);

        var controller = CriarController();

        var resultado = await controller.GetProximas(latitude: -23.5, longitude: -46.6, raioKm: 10);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var clinicas = okResult.Value.Should().BeAssignableTo<IEnumerable<ClinicaVeterinariaDto>>().Subject.ToList();
        clinicas.Should().ContainSingle();
        clinicas.Single().Nome.Should().Be("Clínica Real Um");
        clinicas.Single().DistanciaKm.Should().Be(1.2);
    }

    [Fact]
    public async Task GetProximas_ServicoExternoIndisponivel_Retorna503()
    {
        _searchServiceMock
            .Setup(s => s.BuscarProximasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new VeterinaryClinicSearchException("Overpass API fora do ar"));

        var controller = CriarController();

        var resultado = await controller.GetProximas(latitude: -23.5, longitude: -46.6, raioKm: 10);

        var objectResult = resultado.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task GetProximas_NenhumaClinicaEncontrada_RetornaOkComListaVazia()
    {
        _searchServiceMock
            .Setup(s => s.BuscarProximasAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClinicaVeterinariaEncontrada>());

        var controller = CriarController();

        var resultado = await controller.GetProximas(latitude: -23.5, longitude: -46.6, raioKm: 10);

        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<ClinicaVeterinariaDto>>().Which.Should().BeEmpty();
    }
}