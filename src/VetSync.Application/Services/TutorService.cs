using Microsoft.Extensions.Logging;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using VetSync.Application.Mappings;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class TutorService : ITutorService
{
    public static readonly string[] CamposOrdenaveis = { "id", "nome", "email", "criadoEm" };

    private readonly ITutorRepository _tutores;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TutorService> _logger;

    public TutorService(ITutorRepository tutores, IUnitOfWork unitOfWork, ILogger<TutorService> logger)
    {
        _tutores = tutores;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<TutorDto>> GetPagedAsync(TutorQuery query, CancellationToken ct = default)
    {
        var page = query.ToPageRequest(CamposOrdenaveis, "nome");
        var filter = new TutorFilter { Nome = query.Nome, Email = query.Email, Ativo = query.Ativo };

        var result = await _tutores.SearchAsync(filter, page, ct);
        return PagedResult<TutorDto>.Create(result.Items.Select(t => t.ToDto()).ToList(), result.TotalCount, page);
    }

    public async Task<TutorDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var tutor = await _tutores.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For("Tutor", id);
        return tutor.ToDto();
    }

    public async Task<TutorDto> CreateAsync(CreateTutorDto dto, CancellationToken ct = default)
    {
        var tutor = dto.ToEntity();

        if (await _tutores.EmailExistsAsync(tutor.Email, null, ct))
            throw new ConflictException("Já existe um tutor cadastrado com este e-mail.");
        if (tutor.Cpf is not null && await _tutores.CpfExistsAsync(tutor.Cpf, null, ct))
            throw new ConflictException("Já existe um tutor cadastrado com este CPF.");

        await _tutores.AddAsync(tutor, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Tutor {TutorId} ({TutorNome}) cadastrado.", tutor.Id, tutor.Nome);
        return tutor.ToDto();
    }

    public async Task<TutorDto> UpdateAsync(int id, UpdateTutorDto dto, CancellationToken ct = default)
    {
        var tutor = await _tutores.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For("Tutor", id);

        var email = dto.Email.Trim();
        var cpf = string.IsNullOrWhiteSpace(dto.Cpf) ? null : dto.Cpf.Trim();

        if (await _tutores.EmailExistsAsync(email, id, ct))
            throw new ConflictException("Já existe outro tutor cadastrado com este e-mail.");
        if (cpf is not null && await _tutores.CpfExistsAsync(cpf, id, ct))
            throw new ConflictException("Já existe outro tutor cadastrado com este CPF.");

        tutor.Atualizar(dto.Nome.Trim(), email, dto.Telefone, cpf, dto.Endereco, dto.Ativo);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Tutor {TutorId} atualizado.", id);
        return tutor.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var tutor = await _tutores.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For("Tutor", id);

        if (await _tutores.HasPetsAsync(id, ct))
            throw new ConflictException("Não é possível remover um tutor que possui pets cadastrados.");

        _tutores.Remove(tutor);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation("Tutor {TutorId} removido.", id);
    }
}
