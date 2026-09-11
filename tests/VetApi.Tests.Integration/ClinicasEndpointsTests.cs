using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VetApi.DTOs;
using Xunit;

namespace VetApi.Tests.Integration;

[Collection("Integration Tests")]
public class ClinicasEndpointsTests
{
    private readonly HttpClient _client;

    public ClinicasEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProximas_LocalizacaoNaAvenidaPaulista_RetornaClinicasReaisDoOpenStreetMap()
    {
        const double latitude = -23.561684;
        const double longitude = -46.655981;

        var response = await _client.GetAsync($"/api/clinicas/proximas?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&raioKm=15");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var clinicas = await response.Content.ReadFromJsonAsync<List<ClinicaVeterinariaDto>>();
        clinicas.Should().NotBeNull();

        clinicas!.Should().OnlyContain(c => c.DistanciaKm <= 15);
        clinicas.Should().OnlyContain(c => c.Fonte == "OpenStreetMap");
        clinicas.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Nome));
        clinicas.Should().OnlyContain(c => c.LinkMapa.StartsWith("https://www.openstreetmap.org/"));
        clinicas.Should().BeInAscendingOrder(c => c.DistanciaKm);
    }

    [Theory]
    [InlineData(200, 0)]
    [InlineData(0, 200)]
    public async Task GetProximas_CoordenadasInvalidas_RetornaBadRequest(double latitude, double longitude)
    {

        var response = await _client.GetAsync($"/api/clinicas/proximas?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProximas_RaioAcimaDoMaximoPermitido_RetornaBadRequest()
    {
        const double latitude = -23.561684;
        const double longitude = -46.655981;

        var response = await _client.GetAsync($"/api/clinicas/proximas?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&raioKm=999");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProximas_LocalizacaoNoMeioDoOceano_RetornaListaVaziaSemErro()
    {
        await Task.Delay(2500);
        const double latitude = 0.0;
        const double longitude = -30.0;

        var response = await _client.GetAsync($"/api/clinicas/proximas?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&raioKm=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var clinicas = await response.Content.ReadFromJsonAsync<List<ClinicaVeterinariaDto>>();
        clinicas.Should().NotBeNull().And.BeEmpty();
    }
}
