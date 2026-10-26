using VetSync.Domain.Documents;

namespace VetSync.Application.Abstractions;

/// <summary>
/// Publica eventos da jornada do pet no banco NoSQL. É "best effort": falhas do MongoDB
/// são registradas em log mas nunca derrubam a operação principal gravada no banco relacional.
/// </summary>
public interface IJornadaEventPublisher
{
    Task PublicarAsync(JornadaEvento evento, CancellationToken cancellationToken = default);
}
