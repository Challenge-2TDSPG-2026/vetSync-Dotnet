using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Mappings;
using VetSync.Domain.Documents;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class VacinacaoService : IVacinacaoService
{
    public static readonly string[] CamposOrdenaveis = { "id", "dataAplicacao", "proximaDose", "vacina" };

    private readonly IVacinacaoRepository _vacinacoes;
    private readonly IPetRepository _pets;
    private readonly IJornadaEventPublisher _jornada;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VacinacaoService> _logger;

    public VacinacaoService(IVacinacaoRepository vacinacoes, IPetRepository pets, IJornadaEventPublisher jornada,
        IUnitOfWork unitOfWork, ILogger<VacinacaoService> logger)
    {
        _vacinacoes = vacinacoes;
        _pets = pets;
        _jornada = jornada;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<VacinacaoDto>> GetPagedAsync(VacinacaoQuery query, CancellationToken ct = default)
    {
        var page = query.ToPageRequest(CamposOrdenaveis, "dataAplicacao", descPadrao: true);

        if (query.PetId.HasValue && !await _pets.ExistsAsync(query.PetId.Value, ct))
            throw NotFoundException.For("Pet", query.PetId.Value);

        var filter = new VacinacaoFilter { PetId = query.PetId, ProximaDoseAte = query.ProximaDoseAte };
        var result = await _vacinacoes.SearchAsync(filter, page, ct);
        return PagedResult<VacinacaoDto>.Create(result.Items.Select(v => v.ToDto()).ToList(), result.TotalCount, page);
    }

    public async Task<VacinacaoDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var vacinacao = await _vacinacoes.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Vacinação", id);
        return vacinacao.ToDto();
    }

    public async Task<VacinacaoDto> CreateAsync(CreateVacinacaoDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(dto.PetId, ct) ?? throw NotFoundException.For("Pet", dto.PetId);

        var vacinacao = dto.ToEntity();
        vacinacao.Validar();

        await _vacinacoes.AddAsync(vacinacao, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Vacina {Vacina} registrada para o Pet {PetId}.", vacinacao.Vacina, vacinacao.PetId);

        await _jornada.PublicarAsync(new JornadaEvento
        {
            PetId = pet.Id,
            PetNome = pet.Nome,
            TipoEvento = JornadaEvento.VacinaAplicada,
            Descricao = $"Vacina {vacinacao.Vacina} aplicada",
            ReferenciaId = vacinacao.Id,
            OcorridoEm = vacinacao.DataAplicacao,
            Detalhes = new Dictionary<string, string>
            {
                ["vacina"] = vacinacao.Vacina,
                ["proximaDose"] = vacinacao.ProximaDose?.ToString("yyyy-MM-dd") ?? string.Empty
            }
        }, ct);

        var criada = await _vacinacoes.GetByIdAsync(vacinacao.Id, ct) ?? vacinacao;
        return criada.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var vacinacao = await _vacinacoes.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Vacinação", id);

        _vacinacoes.Remove(vacinacao);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Vacinação {VacinacaoId} removida.", id);
    }
}
