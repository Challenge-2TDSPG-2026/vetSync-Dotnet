using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Mappings;
using VetSync.Domain.Documents;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class ExameService : IExameService
{
    public static readonly string[] CamposOrdenaveis = { "id", "dataExame", "tipoExame" };

    private readonly IExameRepository _exames;
    private readonly IPetRepository _pets;
    private readonly IJornadaEventPublisher _jornada;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExameService> _logger;

    public ExameService(IExameRepository exames, IPetRepository pets, IJornadaEventPublisher jornada,
        IUnitOfWork unitOfWork, ILogger<ExameService> logger)
    {
        _exames = exames;
        _pets = pets;
        _jornada = jornada;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<ExameDto>> GetPagedAsync(ExameQuery query, CancellationToken ct = default)
    {
        var page = query.ToPageRequest(CamposOrdenaveis, "dataExame", descPadrao: true);

        if (query.PetId.HasValue && !await _pets.ExistsAsync(query.PetId.Value, ct))
            throw NotFoundException.For("Pet", query.PetId.Value);

        var filter = new ExameFilter { PetId = query.PetId, Tipo = query.Tipo };
        var result = await _exames.SearchAsync(filter, page, ct);
        return PagedResult<ExameDto>.Create(result.Items.Select(e => e.ToDto()).ToList(), result.TotalCount, page);
    }

    public async Task<ExameDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var exame = await _exames.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Exame", id);
        return exame.ToDto();
    }

    public async Task<ExameDto> CreateAsync(CreateExameDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(dto.PetId, ct) ?? throw NotFoundException.For("Pet", dto.PetId);

        var exame = dto.ToEntity();
        await _exames.AddAsync(exame, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Exame {TipoExame} registrado para o Pet {PetId}.", exame.TipoExame, exame.PetId);

        await _jornada.PublicarAsync(new JornadaEvento
        {
            PetId = pet.Id,
            PetNome = pet.Nome,
            TipoEvento = JornadaEvento.ExameRegistrado,
            Descricao = $"Exame {exame.TipoExame} registrado",
            ReferenciaId = exame.Id,
            OcorridoEm = exame.DataExame,
            Detalhes = new Dictionary<string, string> { ["tipoExame"] = exame.TipoExame }
        }, ct);

        var criado = await _exames.GetByIdAsync(exame.Id, ct) ?? exame;
        return criado.ToDto();
    }

    public async Task<ExameDto> UpdateAsync(int id, UpdateExameDto dto, CancellationToken ct = default)
    {
        var exame = await _exames.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Exame", id);

        exame.Atualizar(dto.TipoExame.Trim(), dto.DataExame, dto.Resultado, dto.Laboratorio, dto.Veterinario);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Exame {ExameId} atualizado.", id);

        await _jornada.PublicarAsync(new JornadaEvento
        {
            PetId = exame.PetId,
            PetNome = exame.Pet?.Nome ?? string.Empty,
            TipoEvento = JornadaEvento.ExameAtualizado,
            Descricao = exame.PossuiResultado
                ? $"Resultado do exame {exame.TipoExame} disponível"
                : $"Exame {exame.TipoExame} atualizado",
            ReferenciaId = exame.Id,
            Detalhes = new Dictionary<string, string> { ["tipoExame"] = exame.TipoExame }
        }, ct);

        return exame.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var exame = await _exames.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Exame", id);

        _exames.Remove(exame);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Exame {ExameId} removido.", id);
    }
}
