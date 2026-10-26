using System.Net.Http.Headers;
using System.Net.Http.Json;
using VetSync.Application.DTOs;

namespace VetSync.Tests.Integration.Fixtures;

/// <summary>Utilitários comuns: clientes autenticados (Admin/Usuario) e criação de massa de dados.</summary>
public abstract class ApiTestBase
{
    protected CustomWebApplicationFactory Factory { get; }

    protected ApiTestBase(CustomWebApplicationFactory factory) => Factory = factory;

    protected HttpClient Anonymous() => Factory.CreateClient();

    protected async Task<HttpClient> AdminClientAsync()
    {
        var client = Factory.CreateClient();
        var resposta = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { Email = CustomWebApplicationFactory.AdminEmail, Senha = CustomWebApplicationFactory.AdminPassword });
        resposta.EnsureSuccessStatusCode();
        var auth = await resposta.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    protected async Task<HttpClient> UserClientAsync()
    {
        var client = Factory.CreateClient();
        var resposta = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Nome = "Usuário Teste",
            Email = $"user-{Guid.NewGuid():N}@vetsync.test",
            Senha = "Senha1234"
        });
        resposta.EnsureSuccessStatusCode();
        var auth = await resposta.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    protected static async Task<TutorDto> CriarTutorAsync(HttpClient client, string? nome = null)
    {
        var resposta = await client.PostAsJsonAsync("/api/tutores", new CreateTutorDto
        {
            Nome = nome ?? "Tutor Teste",
            Email = $"tutor-{Guid.NewGuid():N}@email.com"
        });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<TutorDto>())!;
    }

    protected static async Task<PetDto> CriarPetAsync(HttpClient client, int tutorId, string nome = "Rex", string especie = "Cão")
    {
        var resposta = await client.PostAsJsonAsync("/api/pets", new CreatePetDto { Nome = nome, Especie = especie, TutorId = tutorId });
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<PetDto>())!;
    }
}
