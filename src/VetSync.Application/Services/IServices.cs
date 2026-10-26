using VetSync.Application.Common;
using VetSync.Application.DTOs;

namespace VetSync.Application.Services;

public interface ITutorService
{
    Task<PagedResult<TutorDto>> GetPagedAsync(TutorQuery query, CancellationToken ct = default);
    Task<TutorDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TutorDto> CreateAsync(CreateTutorDto dto, CancellationToken ct = default);
    Task<TutorDto> UpdateAsync(int id, UpdateTutorDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IPetService
{
    Task<PagedResult<PetDto>> GetPagedAsync(PetQuery query, CancellationToken ct = default);
    Task<PetDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<JornadaPetDto> GetJornadaAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<JornadaEventoDto>> GetTimelineAsync(int id, int limite, CancellationToken ct = default);
    Task<PetDto> CreateAsync(CreatePetDto dto, CancellationToken ct = default);
    Task<PetDto> UpdateAsync(int id, UpdatePetDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IConsultaService
{
    Task<PagedResult<ConsultaDto>> GetPagedAsync(ConsultaQuery query, CancellationToken ct = default);
    Task<ConsultaDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ConsultaDto> CreateAsync(CreateConsultaDto dto, CancellationToken ct = default);
    Task<ConsultaDto> UpdateAsync(int id, UpdateConsultaDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IVacinacaoService
{
    Task<PagedResult<VacinacaoDto>> GetPagedAsync(VacinacaoQuery query, CancellationToken ct = default);
    Task<VacinacaoDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<VacinacaoDto> CreateAsync(CreateVacinacaoDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IExameService
{
    Task<PagedResult<ExameDto>> GetPagedAsync(ExameQuery query, CancellationToken ct = default);
    Task<ExameDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ExameDto> CreateAsync(CreateExameDto dto, CancellationToken ct = default);
    Task<ExameDto> UpdateAsync(int id, UpdateExameDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default);
}

public interface IClinicaService
{
    Task<IReadOnlyList<ClinicaVeterinariaDto>> BuscarProximasAsync(
        double latitude, double longitude, double raioKm, CancellationToken ct = default);
}
