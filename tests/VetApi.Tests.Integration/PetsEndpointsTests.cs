using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VetApi.DTOs;
using Xunit;

namespace VetApi.Tests.Integration;

[Collection("Integration Tests")]
public class PetsEndpointsTests
{
    private readonly HttpClient _client;

    public PetsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CriarTutorAsync()
    {
        var dto = new CreateTutorDto
        {
            Nome = "Tutor de Teste",
            Email = $"tutor.{Guid.NewGuid():N}@vetsync.example"
        };

        var response = await _client.PostAsJsonAsync("/api/tutores", dto);
        response.EnsureSuccessStatusCode();

        var tutor = await response.Content.ReadFromJsonAsync<TutorDto>();
        return tutor!.Id;
    }

    [Fact]
    public async Task PostPets_DadosValidos_RetornaCreatedComPetPersistido()
    {
        // Arrange
        var tutorId = await CriarTutorAsync();
        var dto = new CreatePetDto { Nome = "Rex", Especie = "Cachorro", TutorId = tutorId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pets", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var pet = await response.Content.ReadFromJsonAsync<PetDto>();
        pet!.Nome.Should().Be("Rex");
        pet.TutorId.Should().Be(tutorId);
    }

    [Fact]
    public async Task PostPets_TutorIdInexistente_RetornaNotFound()
    {
        // Arrange
        var dto = new CreatePetDto { Nome = "Thor", Especie = "Cachorro", TutorId = 999999 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pets", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostPets_SemNomeObrigatorio_RetornaBadRequest()
    {
        // Arrange
        var tutorId = await CriarTutorAsync();
        var dto = new CreatePetDto { Especie = "Cachorro", TutorId = tutorId };

        // Act
        var response = await _client.PostAsJsonAsync("/api/pets", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPetsById_PetExistente_RetornaOkComOsDadosDoPet()
    {
        // Arrange
        var tutorId = await CriarTutorAsync();
        var criado = await _client.PostAsJsonAsync("/api/pets", new CreatePetDto { Nome = "Luna", Especie = "Gato", TutorId = tutorId });
        var petCriado = await criado.Content.ReadFromJsonAsync<PetDto>();

        // Act
        var response = await _client.GetAsync($"/api/pets/{petCriado!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pet = await response.Content.ReadFromJsonAsync<PetDto>();
        pet!.Nome.Should().Be("Luna");
    }

    [Fact]
    public async Task GetPetsById_PetInexistente_RetornaNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/pets/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePets_PetExistente_RemoveERetornaNoContent()
    {
        // Arrange
        var tutorId = await CriarTutorAsync();
        var criado = await _client.PostAsJsonAsync("/api/pets", new CreatePetDto { Nome = "Bidu", Especie = "Cachorro", TutorId = tutorId });
        var petCriado = await criado.Content.ReadFromJsonAsync<PetDto>();

        // Act
        var respostaDelete = await _client.DeleteAsync($"/api/pets/{petCriado!.Id}");
        var respostaBusca = await _client.GetAsync($"/api/pets/{petCriado.Id}");

        // Assert
        respostaDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        respostaBusca.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPets_QualquerRequisicao_RetornaHeaderDeCorrelationId()
    {
        // Act
        var response = await _client.GetAsync("/api/pets");

        // Assert
        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
    }
}