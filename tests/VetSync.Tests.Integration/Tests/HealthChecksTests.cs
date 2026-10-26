using System.Net;
using FluentAssertions;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class HealthChecksTests : ApiTestBase
{
    public HealthChecksTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetHealthLive_Sempre_RetornaOkComStatusHealthy()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetHealth_Sempre_RetornaJsonComStatusEChecks()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        body.Should().Contain("status").And.Contain("checks");
    }

    [Fact]
    public async Task GetHealth_Sempre_IncluiBancoRelacionalMongoEServicoExterno()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        body.Should().Contain("\"name\": \"self\"");
        body.Should().Contain("\"name\": \"oracle-database\"");
        body.Should().Contain("\"name\": \"mongodb\"");
        body.Should().Contain("\"name\": \"overpass-api\"");
    }
}
