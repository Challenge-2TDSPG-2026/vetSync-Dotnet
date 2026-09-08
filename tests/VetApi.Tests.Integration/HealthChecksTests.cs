using System.Net;
using FluentAssertions;
using Xunit;

namespace VetApi.Tests.Integration;

[Collection("Integration Tests")]
public class HealthChecksTests
{
    private readonly HttpClient _client;

    public HealthChecksTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealthLive_Sempre_RetornaOkComStatusHealthy()
    {

        var response = await _client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetHealth_Sempre_RetornaJsonComTodasAsDependencias()
    {

        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        body.Should().Contain("status");
        body.Should().Contain("checks");
    }

    [Fact]
    public async Task GetHealth_Sempre_IncluiVerificacaoDoBancoEDoServicoExterno()
    {

        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        body.Should().Contain("\"name\": \"self\"");
        body.Should().Contain("\"name\": \"oracle-database\"");
        body.Should().Contain("\"name\": \"overpass-api\"");
    }
}
