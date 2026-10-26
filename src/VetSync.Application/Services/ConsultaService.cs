using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Mappings;
using VetSync.Domain.Documents;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class ConsultaService : IConsultaService
{
    public static readonly string[] CamposOrdenaveis = { "id", "dataConsulta", "status", "veterinario" };

    private readonly IConsultaRepository _consultas;
    private readonly IPetRepository _pets;
    private readonly IJornadaEventPublisher _jornada;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConsultaService> _logger;

    public ConsultaService(IConsultaRepository consultas, IPetRepository pets, IJornadaEventPublisher jornada,
        IUnitOfWork unitOfWork, ILogger<ConsultaService> logger)
    {
        _consultas = consultas;
        _pets = pets;
        _jornada = jornada;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<ConsultaDto>> GetPagedAsync(ConsultaQuery query, CancellationToken ct = default)
    {
        var page = query.ToPageRequest(CamposOrdenaveis, "dataConsulta", descPadrao: true);

        if (!string.IsNullOrWhiteSpace(query.Status) && !Consulta.StatusEhValido(query.Status))
            throw new BusinessRuleException($"Status inválido. Aceitos: {string.Join(", ", Consulta.StatusValidos)}");

        if (query.PetId.HasValue && !await _pets.ExistsAsync(query.PetId.Value, ct))
            throw NotFoundException.For("Pet", query.PetId.Value);

        // "Ate" sem hora representa o dia inteiro.
        var ate = query.Ate.HasValue && query.Ate.Value.TimeOfDay == TimeSpan.Zero
            ? query.Ate.Value.Date.AddDays(1).AddTicks(-1)
            : query.Ate;

        var filter = new ConsultaFilter { PetId = query.PetId, Status = query.Status, De = query.De, Ate = ate };
        var result = await _consultas.SearchAsync(filter, page, ct);
        return PagedResult<ConsultaDto>.Create(result.Items.Select(c => c.ToDto()).ToList(), result.TotalCount, page);
    }

    public async Task<ConsultaDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var consulta = await _consultas.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Consulta", id);
        return consulta.ToDto();
    }

    public async Task<ConsultaDto> CreateAsync(CreateConsultaDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(dto.PetId, ct) ?? throw NotFoundException.For("Pet", dto.PetId);

        var consulta = dto.ToEntity();
        await _consultas.AddAsync(consulta, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Consulta {ConsultaId} agendada para o Pet {PetId}.", consulta.Id, consulta.PetId);

        await _jornada.PublicarAsync(new JornadaEvento
        {
            PetId = pet.Id,
            PetNome = pet.Nome,
            TipoEvento = JornadaEvento.ConsultaAgendada,
            Descricao = $"Consulta agendada com {consulta.Veterinario}",
            ReferenciaId = consulta.Id,
            OcorridoEm = consulta.DataConsulta,
            Detalhes = new Dictionary<string, string>
            {
                ["veterinario"] = consulta.Veterinario,
                ["status"] = consulta.Status
            }
        }, ct);

        var criada = await _consultas.GetByIdAsync(consulta.Id, ct) ?? consulta;
        return criada.ToDto();
    }

    public async Task<ConsultaDto> UpdateAsync(int id, UpdateConsultaDto dto, CancellationToken ct = default)
    {
        var consulta = await _consultas.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Consulta", id);

        consulta.Atualizar(dto.Veterinario.Trim(), dto.DataConsulta, dto.Motivo,
            dto.Diagnostico, dto.Prescricao, dto.Status);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Consulta {ConsultaId} atualizada. Status: {Status}", id, consulta.Status);

        await _jornada.PublicarAsync(new JornadaEvento
        {
            PetId = consulta.PetId,
            PetNome = consulta.Pet?.Nome ?? string.Empty,
            TipoEvento = JornadaEvento.ConsultaAtualizada,
            Descricao = $"Consulta {consulta.Status.ToLowerInvariant()}",
            ReferenciaId = consulta.Id,
            Detalhes = new Dictionary<string, string>
            {
                ["status"] = consulta.Status,
                ["diagnostico"] = consulta.Diagnostico ?? string.Empty
            }
        }, ct);

        return consulta.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var consulta = await _consultas.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Consulta", id);

        _consultas.Remove(consulta);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Consulta {ConsultaId} removida.", id);
    }
}
