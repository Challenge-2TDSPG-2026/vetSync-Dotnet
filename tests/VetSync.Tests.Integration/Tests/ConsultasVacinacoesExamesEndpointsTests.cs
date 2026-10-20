using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Tests.Integration.Fixtures;
using Xunit;

namespace VetSync.Tests.Integration.Tests;

[Collection(IntegrationTestCollection.Name)]
public class ConsultasEndpointsTests : ApiTestBase
{
    public ConsultasEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<(HttpClient Client, PetDto Pet)> PrepararPetAsync()
    {
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        return (client, await CriarPetAsync(client, tutor.Id));
    }

    [Fact]
    public async Task Post_PetExistente_RetornaCreatedComStatusAgendada()
    {
        // Arrange
        var (client, pet) = await PrepararPetAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/consultas",
            new CreateConsultaDto { PetId = pet.Id, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow.AddDays(2) });
        var consulta = await response.Content.ReadFromJsonAsync<ConsultaDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        consulta!.Status.Should().Be("Agendada");
        consulta.PetNome.Should().Be("Rex");
    }

    [Fact]
    public async Task Post_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/consultas",
            new CreateConsultaDto { PetId = 999999, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_StatusInvalido_RetornaBadRequest()
    {
        // Arrange
        var (client, pet) = await PrepararPetAsync();
        var criada = await (await client.PostAsJsonAsync("/api/consultas",
            new CreateConsultaDto { PetId = pet.Id, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow })).Content.ReadFromJsonAsync<ConsultaDto>();

        // Act
        var response = await client.PutAsJsonAsync($"/api/consultas/{criada!.Id}",
            new UpdateConsultaDto { Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow, Status = "Inventado" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_StatusRealizada_PersisteAlteracao()
    {
        // Arrange
        var (client, pet) = await PrepararPetAsync();
        var criada = await (await client.PostAsJsonAsync("/api/consultas",
            new CreateConsultaDto { PetId = pet.Id, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow })).Content.ReadFromJsonAsync<ConsultaDto>();

        // Act
        var put = await client.PutAsJsonAsync($"/api/consultas/{criada!.Id}",
            new UpdateConsultaDto { Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow, Status = "Realizada", Diagnostico = "Saudável" });
        var get = await client.GetFromJsonAsync<ConsultaDto>($"/api/consultas/{criada.Id}");

        // Assert
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);
        get!.Status.Should().Be("Realizada");
        get.Diagnostico.Should().Be("Saudável");
    }

    [Fact]
    public async Task GetAll_FiltroPorStatusEPet_RetornaSomenteCorrespondentes()
    {
        // Arrange
        var (client, pet) = await PrepararPetAsync();
        await client.PostAsJsonAsync("/api/consultas", new CreateConsultaDto { PetId = pet.Id, Veterinario = "A", DataConsulta = DateTime.UtcNow });
        await client.PostAsJsonAsync("/api/consultas", new CreateConsultaDto { PetId = pet.Id, Veterinario = "B", DataConsulta = DateTime.UtcNow.AddDays(1) });

        // Act
        var pagina = await client.GetFromJsonAsync<PagedResult<ConsultaDto>>($"/api/consultas?petId={pet.Id}&status=Agendada&sortBy=veterinario&sortDir=desc");

        // Assert
        pagina!.TotalCount.Should().Be(2);
        pagina.Items.Select(c => c.Veterinario).Should().ContainInOrder("B", "A");
        pagina.Items.Should().OnlyContain(c => c.Status == "Agendada");
    }

    [Fact]
    public async Task GetAll_StatusInvalido_RetornaBadRequest()
    {
        // Arrange
        var client = await UserClientAsync();

        // Act
        var response = await client.GetAsync("/api/consultas?status=Nada");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_Admin_RemoveConsulta()
    {
        // Arrange
        var admin = await AdminClientAsync();
        var tutor = await CriarTutorAsync(admin);
        var pet = await CriarPetAsync(admin, tutor.Id);
        var criada = await (await admin.PostAsJsonAsync("/api/consultas",
            new CreateConsultaDto { PetId = pet.Id, Veterinario = "A", DataConsulta = DateTime.UtcNow })).Content.ReadFromJsonAsync<ConsultaDto>();

        // Act
        var delete = await admin.DeleteAsync($"/api/consultas/{criada!.Id}");
        var get = await admin.GetAsync($"/api/consultas/{criada.Id}");

        // Assert
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

[Collection(IntegrationTestCollection.Name)]
public class VacinacoesEndpointsTests : ApiTestBase
{
    public VacinacoesEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Post_ProximaDoseAnteriorAAplicacao_RetornaBadRequest()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/vacinacoes", new CreateVacinacaoDto
        {
            PetId = pet.Id, Vacina = "V10", DataAplicacao = new DateTime(2026, 5, 10), ProximaDose = new DateTime(2026, 4, 1)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_FiltroProximaDoseAte_RetornaSomenteVacinasComDoseAteADataInformada()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);
        await client.PostAsJsonAsync("/api/vacinacoes", new CreateVacinacaoDto
        { PetId = pet.Id, Vacina = "Curta", DataAplicacao = new DateTime(2026, 1, 1), ProximaDose = new DateTime(2026, 2, 1) });
        await client.PostAsJsonAsync("/api/vacinacoes", new CreateVacinacaoDto
        { PetId = pet.Id, Vacina = "Longa", DataAplicacao = new DateTime(2026, 1, 1), ProximaDose = new DateTime(2027, 1, 1) });

        // Act
        var pagina = await client.GetFromJsonAsync<PagedResult<VacinacaoDto>>($"/api/vacinacoes?petId={pet.Id}&proximaDoseAte=2026-03-01");

        // Assert
        pagina!.Items.Should().ContainSingle().Which.Vacina.Should().Be("Curta");
    }
}

[Collection(IntegrationTestCollection.Name)]
public class ExamesEndpointsTests : ApiTestBase
{
    public ExamesEndpointsTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Put_LancarResultado_PersisteEPublicaEventoNaTimeline()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);
        var exame = await (await client.PostAsJsonAsync("/api/exames",
            new CreateExameDto { PetId = pet.Id, TipoExame = "Hemograma", DataExame = DateTime.UtcNow })).Content.ReadFromJsonAsync<ExameDto>();

        // Act
        var put = await client.PutAsJsonAsync($"/api/exames/{exame!.Id}",
            new UpdateExameDto { TipoExame = "Hemograma", DataExame = DateTime.UtcNow, Resultado = "Dentro da normalidade" });
        var get = await client.GetFromJsonAsync<ExameDto>($"/api/exames/{exame.Id}");
        var timeline = await client.GetFromJsonAsync<List<JornadaEventoDto>>($"/api/pets/{pet.Id}/timeline");

        // Assert
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);
        get!.Resultado.Should().Be("Dentro da normalidade");
        timeline!.Should().Contain(e => e.TipoEvento == "ExameAtualizado");
    }

    [Fact]
    public async Task GetAll_FiltroPorTipo_RetornaSomenteExamesDoTipo()
    {
        // Arrange
        var client = await UserClientAsync();
        var tutor = await CriarTutorAsync(client);
        var pet = await CriarPetAsync(client, tutor.Id);
        await client.PostAsJsonAsync("/api/exames", new CreateExameDto { PetId = pet.Id, TipoExame = "Hemograma", DataExame = DateTime.UtcNow });
        await client.PostAsJsonAsync("/api/exames", new CreateExameDto { PetId = pet.Id, TipoExame = "Raio-X", DataExame = DateTime.UtcNow });

        // Act
        var pagina = await client.GetFromJsonAsync<PagedResult<ExameDto>>($"/api/exames?petId={pet.Id}&tipo=raio");

        // Assert
        pagina!.Items.Should().ContainSingle().Which.TipoExame.Should().Be("Raio-X");
    }
}
