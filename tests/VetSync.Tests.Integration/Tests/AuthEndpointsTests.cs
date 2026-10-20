using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using VetSync.Application.DTOs;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class AuthEndpointsTests : ApiTestBase
{
    public AuthEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_DadosValidos_RetornaCreatedComToken()
    {
        // Arrange
        var client = Anonymous();
        var dto = new RegisterDto { Nome = "Maria", Email = $"maria-{Guid.NewGuid():N}@vet.com", Senha = "Senha1234" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", dto);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Usuario");
        body.TipoToken.Should().Be("Bearer");
    }

    [Fact]
    public async Task Register_EmailDuplicado_RetornaConflictComProblemDetails()
    {
        // Arrange
        var client = Anonymous();
        var dto = new RegisterDto { Nome = "Maria", Email = $"dup-{Guid.NewGuid():N}@vet.com", Senha = "Senha1234" };
        await client.PostAsJsonAsync("/api/auth/register", dto);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", dto);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        problem!.Status.Should().Be(409);
    }

    [Fact]
    public async Task Register_SenhaFraca_RetornaBadRequest()
    {
        // Arrange
        var client = Anonymous();
        var dto = new RegisterDto { Nome = "Maria", Email = $"fraca-{Guid.NewGuid():N}@vet.com", Senha = "somenteletras" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_AdminSemeado_RetornaTokenComPerfilAdmin()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = CustomWebApplicationFactory.AdminEmail, Senha = CustomWebApplicationFactory.AdminPassword });
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_SenhaIncorreta_RetornaUnauthorized()
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = CustomWebApplicationFactory.AdminEmail, Senha = "SenhaErrada1" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/tutores")]
    [InlineData("/api/pets")]
    [InlineData("/api/consultas")]
    [InlineData("/api/vacinacoes")]
    [InlineData("/api/exames")]
    public async Task GetColecao_SemToken_RetornaUnauthorized(string rota)
    {
        // Arrange
        var client = Anonymous();

        // Act
        var response = await client.GetAsync(rota);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetColecao_TokenInvalido_RetornaUnauthorized()
    {
        // Arrange
        var client = Anonymous();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token.invalido.aqui");

        // Act
        var response = await client.GetAsync("/api/tutores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetColecao_ComTokenDeUsuario_RetornaOk()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/tutores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
