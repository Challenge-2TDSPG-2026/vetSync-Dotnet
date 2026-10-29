using System.Net;
using FluentAssertions;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class SwaggerTests
{
    private readonly CustomWebApplicationFactory _factory;

    public SwaggerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetSwaggerJson_Anonimo_RetornaEspecificacaoOpenApiComJwt()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        json.Should().Contain("\"/api/pets\"");
        json.Should().Contain("\"/api/auth/login\"");
        json.Should().Contain("\"bearer\"");

        // Exporta a especificacao quando EXPORT_SWAGGER_PATH estiver definida (usado para gerar docs/swagger.json)
        var destino = Environment.GetEnvironmentVariable("EXPORT_SWAGGER_PATH");
        if (!string.IsNullOrWhiteSpace(destino))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
            await File.WriteAllTextAsync(destino, json);
        }
    }
}