using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class TutoresEndpointsTests : ApiTestBase
{
    public TutoresEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_DadosValidos_RetornaCreatedComLocationELinksHateoas()
    {
        // Arrange
        var client = await UserClientAsync();
        var dto = new CreateTutorDto { Nome = "Maria Souza", Email = $"m-{Guid.NewGuid():N}@email.com" };

        // Act
        var response = await client.PostAsJsonAsync("/api/tutores", dto);
        var criado = await response.Content.ReadFromJsonAsync<TutorDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        criado!.Links.Select(l => l.Rel).Should().Contain(new[] { "self", "update", "delete", "pets" });
    }

    [Fact]
    public async Task Post_EmailDuplicado_RetornaConflict()
    {
        // Arrange
        var client = await UserClientAsync();
        var email = $"dup-{Guid.NewGuid():N}@email.com";
        await client.PostAsJsonAsync("/api/tutores", new CreateTutorDto { Nome = "A", Email = email });

        // Act
        var response = await client.PostAsJsonAsync("/api/tutores", new CreateTutorDto { Nome = "B", Email = email });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_EmailInvalido_RetornaBadRequest()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/tutores", new CreateTutorDto { Nome = "A", Email = "nao-e-email" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_Inexistente_RetornaNotFoundComProblemDetails()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/tutores/987654");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem!.Status.Should().Be(404);
        problem.Title.Should().Be("Recurso não encontrado");
    }

    [Fact]
    public async Task GetAll_ComPageSizeDois_RetornaPaginaComLinksDeNavegacao()
    {
        // Arrange
        var client = await UserClientAsync();
        var prefixo = $"Paginado{Guid.NewGuid():N}";
        for (var i = 1; i <= 3; i++)
            await CriarTutorAsync(client, $"{prefixo} {i}");

        // Act
        var response = await client.GetAsync($"/api/tutores?nome={prefixo}&page=1&pageSize=2&sortBy=nome&sortDir=asc");
        var pagina = await response.Content.ReadFromJsonAsync<PagedResult<TutorDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        pagina!.Items.Should().HaveCount(2);
        pagina.TotalCount.Should().Be(3);
        pagina.TotalPages.Should().Be(2);
        pagina.Items.Select(t => t.Nome).Should().BeInAscendingOrder();
        pagina.Links.Select(l => l.Rel).Should().Contain(new[] { "self", "first", "next", "last" });
        pagina.Links.Select(l => l.Rel).Should().NotContain("prev");
        pagina.Links.Single(l => l.Rel == "next").Href.Should().Contain("page=2").And.Contain($"nome={prefixo}");
    }

    [Fact]
    public async Task GetAll_SegundaPaginaDescendente_RetornaRestanteEmOrdemInversa()
    {
        // Arrange
        var client = await UserClientAsync();
        var prefixo = $"Ordem{Guid.NewGuid():N}";
        for (var i = 1; i <= 3; i++)
            await CriarTutorAsync(client, $"{prefixo} {i}");

        // Act
        var response = await client.GetAsync($"/api/tutores?nome={prefixo}&page=2&pageSize=2&sortBy=nome&sortDir=desc");
        var pagina = await response.Content.ReadFromJsonAsync<PagedResult<TutorDto>>();

        // Assert
        pagina!.Items.Should().ContainSingle().Which.Nome.Should().EndWith(" 1");
        pagina.Links.Select(l => l.Rel).Should().Contain("prev");
        pagina.Links.Select(l => l.Rel).Should().NotContain("next");
    }

    [Fact]
    public async Task GetAll_CampoDeOrdenacaoInvalido_RetornaBadRequest()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/tutores?sortBy=senha");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_PageSizeAcimaDoMaximo_EhLimitadoACem()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/tutores?pageSize=100000");
        var pagina = await response.Content.ReadFromJsonAsync<PagedResult<TutorDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        pagina!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task Put_TutorExistente_RetornaNoContentEPersisteAlteracao()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);

        // Act
        var put = await client.PutAsJsonAsync($"/api/tutores/{tutor.Id}", new UpdateTutorDto
        {
            Nome = "Nome Atualizado", Email = tutor.Email, Ativo = true
        });
        var get = await client.GetFromJsonAsync<TutorDto>($"/api/tutores/{tutor.Id}");

        // Assert
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);
        get!.Nome.Should().Be("Nome Atualizado");
        get.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_UsuarioComum_RetornaForbidden()
    {
        // Arrange
        var user = await UserClientAsync();
        var tutor = await CriarTutorAsync(user);

        // Act
        var response = await user.DeleteAsync($"/api/tutores/{tutor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Admin_RemoveTutorERetornaNoContent()
    {
        // Arrange
        var admin = await AdminClientAsync();
        var tutor = await CriarTutorAsync(admin);

        // Act
        var delete = await admin.DeleteAsync($"/api/tutores/{tutor.Id}");
        var get = await admin.GetAsync($"/api/tutores/{tutor.Id}");

        // Assert
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_TutorComPets_RetornaConflict()
    {
        // Arrange
        var admin = await AdminClientAsync();
        var tutor = await CriarTutorAsync(admin);
        await CriarPetAsync(admin, tutor.Id);

        // Act
        var response = await admin.DeleteAsync($"/api/tutores/{tutor.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
