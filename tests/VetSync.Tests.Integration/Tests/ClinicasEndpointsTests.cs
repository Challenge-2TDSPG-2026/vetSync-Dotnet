using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VetSync.Application.DTOs;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class ClinicasEndpointsTests : ApiTestBase
{
    private const double Latitude = -23.561684;
    private const double Longitude = -46.655981;

    public ClinicasEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    private static string Url(double lat, double lon, double? raio = null)
        => $"/api/clinicas/proximas?latitude={lat.ToString(CultureInfo.InvariantCulture)}&longitude={lon.ToString(CultureInfo.InvariantCulture)}"
           + (raio.HasValue ? $"&raioKm={raio.Value.ToString(CultureInfo.InvariantCulture)}" : string.Empty);

    [Fact]
    public async Task GetProximas_SemAutenticacao_RetornaClinicasOrdenadasPorDistancia()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync(Url(Latitude, Longitude, 15));
        var clinicas = await response.Content.ReadFromJsonAsync<List<ClinicaVeterinariaDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        clinicas.Should().NotBeNullOrEmpty();
        clinicas!.Should().OnlyContain(c => c.DistanciaKm <= 15);
        clinicas.Should().BeInAscendingOrder(c => c.DistanciaKm);
        clinicas.Should().OnlyContain(c => c.LinkMapa.StartsWith("https://www.openstreetmap.org/"));
    }

    [Theory]
    [InlineData(200, 0)]
    [InlineData(0, 200)]
    public async Task GetProximas_CoordenadasInvalidas_RetornaBadRequest(double latitude, double longitude)
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync(Url(latitude, longitude));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProximas_RaioAcimaDoMaximoPermitido_RetornaBadRequest()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync(Url(Latitude, Longitude, 999));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
