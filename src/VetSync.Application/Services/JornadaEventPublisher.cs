using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Domain.Documents;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

/// <summary>Grava eventos no MongoDB sem nunca propagar falhas para a operação principal.</summary>
public class JornadaEventPublisher : IJornadaEventPublisher
{
    private readonly IJornadaEventoRepository _repository;
    private readonly ILogger<JornadaEventPublisher> _logger;

    public JornadaEventPublisher(IJornadaEventoRepository repository, ILogger<JornadaEventPublisher> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task PublicarAsync(JornadaEvento evento, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.AddAsync(evento, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex,
                "Não foi possível registrar o evento {TipoEvento} do Pet {PetId} no MongoDB.",
                evento.TipoEvento, evento.PetId);
        }
    }
}
