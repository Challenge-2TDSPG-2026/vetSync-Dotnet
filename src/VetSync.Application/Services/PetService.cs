using Microsoft.Extensions.Logging;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Mappings;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class PetService : IPetService
{
    public static readonly string[] CamposOrdenaveis = { "id", "nome", "especie", "raca", "criadoEm" };
    private const int LimiteMaximoTimeline = 200;

    private readonly IPetRepository _pets;
    private readonly ITutorRepository _tutores;
    private readonly IJornadaEventoRepository _jornada;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PetService> _logger;

    public PetService(IPetRepository pets, ITutorRepository tutores, IJornadaEventoRepository jornada,
        IUnitOfWork unitOfWork, ILogger<PetService> logger)
    {
        _pets = pets;
        _tutores = tutores;
        _jornada = jornada;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<PetDto>> GetPagedAsync(PetQuery query, CancellationToken ct = default)
    {
        var page = query.ToPageRequest(CamposOrdenaveis, "nome");

        if (query.TutorId.HasValue && !await _tutores.ExistsAsync(query.TutorId.Value, ct))
            throw NotFoundException.For("Tutor", query.TutorId.Value);

        var filter = new PetFilter
        {
            Nome = query.Nome, Especie = query.Especie, Raca = query.Raca,
            Ativo = query.Ativo, TutorId = query.TutorId
        };

        var result = await _pets.SearchAsync(filter, page, ct);
        return PagedResult<PetDto>.Create(result.Items.Select(p => p.ToDto()).ToList(), result.TotalCount, page);
    }

    public async Task<PetDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Pet", id);
        return pet.ToDto();
    }

    public async Task<JornadaPetDto> GetJornadaAsync(int id, CancellationToken ct = default)
    {
        var pet = await _pets.GetWithJornadaAsync(id, ct) ?? throw NotFoundException.For("Pet", id);

        return new JornadaPetDto
        {
            Pet = pet.ToDto(),
            Tutor = pet.Tutor.ToDto(),
            Consultas = pet.Consultas.OrderByDescending(c => c.DataConsulta).Select(c => c.ToDto()).ToList(),
            Vacinacoes = pet.Vacinacoes.OrderByDescending(v => v.DataAplicacao).Select(v => v.ToDto()).ToList(),
            Exames = pet.Exames.OrderByDescending(e => e.DataExame).Select(e => e.ToDto()).ToList()
        };
    }

    public async Task<IReadOnlyList<JornadaEventoDto>> GetTimelineAsync(int id, int limite, CancellationToken ct = default)
    {
        if (!await _pets.ExistsAsync(id, ct))
            throw NotFoundException.For("Pet", id);

        var limiteEfetivo = Math.Clamp(limite, 1, LimiteMaximoTimeline);
        var eventos = await _jornada.GetByPetAsync(id, limiteEfetivo, ct);
        return eventos.Select(e => e.ToDto()).ToList();
    }

    public async Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct = default)
    {
        if (!await _tutores.ExistsAsync(dto.TutorId, ct))
        {
            _logger.LogWarning("Tentativa de cadastrar pet para TutorId {TutorId} inexistente.", dto.TutorId);
            throw NotFoundException.For("Tutor", dto.TutorId);
        }

        var pet = dto.ToEntity();
        await _pets.AddAsync(pet, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Pet {PetId} ({PetNome}) cadastrado para o Tutor {TutorId}.", pet.Id, pet.Nome, pet.TutorId);

        var criado = await _pets.GetByIdAsync(pet.Id, ct) ?? pet;
        return criado.ToDto();
    }

    public async Task<PetDto> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Pet", id);

        pet.Atualizar(dto.Nome.Trim(), dto.Especie.Trim(), dto.Raca, dto.Sexo,
            dto.DataNascimento, dto.Peso, dto.Cor, dto.Ativo);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Pet {PetId} atualizado.", id);
        return pet.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var pet = await _pets.GetByIdAsync(id, ct) ?? throw NotFoundException.For("Pet", id);

        _pets.Remove(pet);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Pet {PetId} removido.", id);
    }
}
