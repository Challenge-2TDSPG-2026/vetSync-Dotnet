using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Domain.Documents;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class PetsEndpointsTests : ApiTestBase
{
    public PetsEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_TutorInexistente_RetornaNotFound()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/pets", new CreatePetDto { Nome = "Rex", Especie = "Cão", TutorId = 999999 });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_DadosValidos_RetornaPetComNomeDoTutorELinks()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client, "Dona Maria");

        // Act
        var pet = await CriarPetAsync(client, tutor.Id, "Thor");

        // Assert
        pet.TutorNome.Should().Be("Dona Maria");
        pet.Links.Select(l => l.Rel).Should().Contain(new[] { "self", "tutor", "jornada", "timeline", "consultas", "vacinacoes", "exames" });
    }

    [Fact]
    public async Task GetAll_FiltroPorTutorIdEEspecie_RetornaSomenteOsPetsDoTutor()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        await CriarPetAsync(client, tutor.Id, "Rex", "Cão");
        await CriarPetAsync(client, tutor.Id, "Mia", "Gato");
        var outro = await CriarTutorAsync(client);
        await CriarPetAsync(client, outro.Id, "Bob", "Cão");

        // Act
        var todos = await client.GetFromJsonAsync<PagedResult<PetDto>>($"/api/pets?tutorId={tutor.Id}");
        var gatos = await client.GetFromJsonAsync<PagedResult<PetDto>>($"/api/pets?tutorId={tutor.Id}&especie=gato");

        // Assert
        todos!.TotalCount.Should().Be(2);
        todos.Items.Should().OnlyContain(p => p.TutorId == tutor.Id);
        gatos!.Items.Should().ContainSingle().Which.Nome.Should().Be("Mia");
    }

    [Fact]
    public async Task GetAll_TutorIdInexistente_RetornaNotFound()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/pets?tutorId=999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetJornada_PetComConsultaVacinaEExame_RetornaHistoricoCompleto()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);
        await client.PostAsJsonAsync("/api/consultas", new CreateConsultaDto { PetId = pet.Id, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow.AddDays(1) });
        await client.PostAsJsonAsync("/api/vacinacoes", new CreateVacinacaoDto { PetId = pet.Id, Vacina = "V10", DataAplicacao = DateTime.UtcNow });
        await client.PostAsJsonAsync("/api/exames", new CreateExameDto { PetId = pet.Id, TipoExame = "Hemograma", DataExame = DateTime.UtcNow });

        // Act
        var jornada = await client.GetFromJsonAsync<JornadaPetDto>($"/api/pets/{pet.Id}/jornada");

        // Assert
        jornada!.Pet.Id.Should().Be(pet.Id);
        jornada.Tutor.Id.Should().Be(tutor.Id);
        jornada.Consultas.Should().HaveCount(1);
        jornada.Vacinacoes.Should().HaveCount(1);
        jornada.Exames.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTimeline_AposRegistrarEventos_RetornaEventosGravadosNoNoSql()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);
        await client.PostAsJsonAsync("/api/vacinacoes", new CreateVacinacaoDto { PetId = pet.Id, Vacina = "Antirrábica", DataAplicacao = DateTime.UtcNow });
        await client.PostAsJsonAsync("/api/exames", new CreateExameDto { PetId = pet.Id, TipoExame = "Urina", DataExame = DateTime.UtcNow.AddMinutes(1) });

        // Act
        var response = await client.GetAsync($"/api/pets/{pet.Id}/timeline");
        var eventos = await response.Content.ReadFromJsonAsync<List<JornadaEventoDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        eventos!.Select(e => e.TipoEvento).Should().Contain(new[] { JornadaEvento.VacinaAplicada, JornadaEvento.ExameRegistrado });
        eventos.Should().OnlyContain(e => e.PetId == pet.Id && e.PetNome == "Rex");
        eventos.Should().BeInDescendingOrder(e => e.OcorridoEm);
    }

    [Fact]
    public async Task GetTimeline_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/pets/999999/timeline");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_UsuarioComum_RetornaForbidden()
    {
        // Arrange
        var user = await UserClientAsync();
        var tutor = await CriarTutorAsync(user);
        var pet = await CriarPetAsync(user, tutor.Id);

        // Act
        var response = await user.DeleteAsync($"/api/pets/{pet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
