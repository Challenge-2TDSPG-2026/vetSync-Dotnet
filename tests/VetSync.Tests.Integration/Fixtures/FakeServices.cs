using System.Collections.Concurrent;
using VetSync.Application.Abstractions;
using VetSync.Domain.Documents;
using VetSync.Domain.Repositories;

namespace VetSync.Tests.Integration.Fixtures;

/// <summary>Substitui o MongoDB nos testes de integração, mantendo o mesmo contrato.</summary>
public class InMemoryJornadaEventoRepository : IJornadaEventoRepository
{
    private readonly ConcurrentBag<JornadaEvento> _eventos = new();

    public IReadOnlyCollection<JornadaEvento> Todos => _eventos.ToArray();

    public Task AddAsync(JornadaEvento evento, CancellationToken cancellationToken = default)
    {
        _eventos.Add(evento);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<JornadaEvento>> GetByPetAsync(int petId, int limite, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<JornadaEvento> resultado = _eventos
            .Where(e => e.PetId == petId)
            .OrderByDescending(e => e.OcorridoEm)
            .Take(limite)
            .ToList();
        return Task.FromResult(resultado);
    }
}

/// <summary>Evita depender da API pública do OpenStreetMap nos testes (determinístico e offline).</summary>
public class FakeVeterinaryClinicSearchService : IVeterinaryClinicSearchService
{
    public Task<List<ClinicaVeterinariaEncontrada>> BuscarProximasAsync(
        double latitude, double longitude, double raioKm, CancellationToken cancellationToken = default)
    {
        var lista = new List<ClinicaVeterinariaEncontrada>
        {
            new() { Id = "node/1", Nome = "Vet Paulista", DistanciaKm = 0.8, Latitude = latitude, Longitude = longitude },
            new() { Id = "node/2", Nome = "Pet Centro", DistanciaKm = 2.4, Latitude = latitude, Longitude = longitude }
        };
        return Task.FromResult(lista.Where(c => c.DistanciaKm <= raioKm).ToList());
    }
}
