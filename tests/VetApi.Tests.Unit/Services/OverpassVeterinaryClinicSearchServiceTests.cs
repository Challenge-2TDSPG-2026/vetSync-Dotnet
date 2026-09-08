using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using VetApi.Services;
using Xunit;

namespace VetApi.Tests.Unit.Services;

[Collection("GeoLocationService Collection")]
public class OverpassVeterinaryClinicSearchServiceTests
{
    private readonly GeoLocationServiceFixture _geoFixture;
    private readonly Mock<ILogger<OverpassVeterinaryClinicSearchService>> _loggerMock = new();

    public OverpassVeterinaryClinicSearchServiceTests(GeoLocationServiceFixture geoFixture)
    {
        _geoFixture = geoFixture;
    }

    private const string RespostaOverpassComDuasClinicas = @"{
        ""version"": 0.6,
        ""elements"": [
            {
                ""type"": ""node"",
                ""id"": 111,
                ""lat"": -23.561684,
                ""lon"": -46.655981,
                ""tags"": {
                    ""amenity"": ""veterinary"",
                    ""name"": ""Clínica Pet Amigo"",
                    ""phone"": ""+55 11 5555-1234"",
                    ""addr:street"": ""Av. Paulista"",
                    ""addr:housenumber"": ""1000"",
                    ""addr:city"": ""São Paulo""
                }
            },
            {
                ""type"": ""way"",
                ""id"": 222,
                ""center"": { ""lat"": -23.566389, ""lon"": -46.686389 },
                ""tags"": {
                    ""amenity"": ""veterinary"",
                    ""name"": ""VetCare Pinheiros""
                }
            }
        ]
    }";

    private IConfiguration ConfiguracaoComEndpoint(string endpoint)
    {
        var dict = new Dictionary<string, string?> { ["Overpass:Endpoints:0"] = endpoint };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private OverpassVeterinaryClinicSearchService CriarServico(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        return new OverpassVeterinaryClinicSearchService(
            httpClient,
            _geoFixture.Service,
            ConfiguracaoComEndpoint("http://fake-overpass.test/api/interpreter"),
            _loggerMock.Object);
    }

    [Fact]
    public async Task BuscarProximasAsync_RespostaValidaComClinicasReaisNoRaio_RetornaOrdenadasPorDistancia()
    {
        var handler = new FakeHttpMessageHandler(RespostaOverpassComDuasClinicas);
        var servico = CriarServico(handler);

        var resultado = await servico.BuscarProximasAsync(latitude: -23.561684, longitude: -46.655981, raioKm: 10);

        resultado.Should().HaveCount(2);
        resultado[0].Nome.Should().Be("Clínica Pet Amigo");
        resultado[0].DistanciaKm.Should().Be(0);
        resultado[1].Nome.Should().Be("VetCare Pinheiros");
        resultado[1].DistanciaKm.Should().BeGreaterThan(0);
        resultado.Should().BeInAscendingOrder(c => c.DistanciaKm);
    }

    [Fact]
    public async Task BuscarProximasAsync_ClinicaComEndereco_MontaEnderecoAPartirDasTagsOsm()
    {
        var handler = new FakeHttpMessageHandler(RespostaOverpassComDuasClinicas);
        var servico = CriarServico(handler);

        var resultado = await servico.BuscarProximasAsync(-23.561684, -46.655981, 10);

        var petAmigo = resultado.Single(c => c.Nome == "Clínica Pet Amigo");
        petAmigo.Endereco.Should().Contain("Av. Paulista, 1000");
        petAmigo.Endereco.Should().Contain("São Paulo");
        petAmigo.Telefone.Should().Be("+55 11 5555-1234");
        petAmigo.Id.Should().Be("node/111");
        petAmigo.LinkMapa.Should().Be("https://www.openstreetmap.org/node/111");
    }

    [Fact]
    public async Task BuscarProximasAsync_SemElementosNaResposta_RetornaListaVazia()
    {
        var handler = new FakeHttpMessageHandler(@"{ ""version"": 0.6, ""elements"": [] }");
        var servico = CriarServico(handler);

        var resultado = await servico.BuscarProximasAsync(-23.5, -46.6, 10);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarProximasAsync_ApiExternaRetornaErroHttp_LancaVeterinaryClinicSearchException()
    {
        var handler = new FakeHttpMessageHandler("erro interno", HttpStatusCode.InternalServerError);
        var servico = CriarServico(handler);

        var act = async () => await servico.BuscarProximasAsync(-23.5, -46.6, 10);

        await act.Should().ThrowAsync<VeterinaryClinicSearchException>();
    }

    [Fact]
    public async Task BuscarProximasAsync_ConsultaEnviada_ContemCoordenadasERaioInformados()
    {
        var handler = new FakeHttpMessageHandler(@"{ ""elements"": [] }");
        var servico = CriarServico(handler);

        await servico.BuscarProximasAsync(latitude: -23.5, longitude: -46.6, raioKm: 5);

        handler.UltimoCorpoRequisicao.Should().Contain("-23.5");
        handler.UltimoCorpoRequisicao.Should().Contain("-46.6");
        handler.UltimoCorpoRequisicao.Should().Contain("5000");
        handler.UltimoCorpoRequisicao.Should().Contain("amenity");
        handler.UltimoCorpoRequisicao.Should().Contain("veterinary");
    }
}
