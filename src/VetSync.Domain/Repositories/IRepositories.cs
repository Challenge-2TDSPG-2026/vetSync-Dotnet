using VetSync.Domain.Documents;
using VetSync.Domain.Entities;

namespace VetSync.Domain.Repositories;

/// <summary>Confirma (commit) todas as alterações pendentes em uma única transação.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Contrato base: leitura por ID (entidade rastreada), inclusão e remoção.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Remove(T entity);
}

public interface ITutorRepository : IRepository<Tutor>
{
    Task<PagedList<Tutor>> SearchAsync(TutorFilter filter, PageRequest page, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? ignorarId = null, CancellationToken cancellationToken = default);
    Task<bool> CpfExistsAsync(string cpf, int? ignorarId = null, CancellationToken cancellationToken = default);
    Task<bool> HasPetsAsync(int tutorId, CancellationToken cancellationToken = default);
}

public interface IPetRepository : IRepository<Pet>
{
    Task<PagedList<Pet>> SearchAsync(PetFilter filter, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>Pet com Tutor, Consultas, Vacinações e Exames carregados.</summary>
    Task<Pet?> GetWithJornadaAsync(int id, CancellationToken cancellationToken = default);
}

public interface IConsultaRepository : IRepository<Consulta>
{
    Task<PagedList<Consulta>> SearchAsync(ConsultaFilter filter, PageRequest page, CancellationToken cancellationToken = default);
}

public interface IVacinacaoRepository : IRepository<Vacinacao>
{
    Task<PagedList<Vacinacao>> SearchAsync(VacinacaoFilter filter, PageRequest page, CancellationToken cancellationToken = default);
}

public interface IExameRepository : IRepository<Exame>
{
    Task<PagedList<Exame>> SearchAsync(ExameFilter filter, PageRequest page, CancellationToken cancellationToken = default);
}

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>Repositório NoSQL (MongoDB) da linha do tempo de eventos do pet.</summary>
public interface IJornadaEventoRepository
{
    Task AddAsync(JornadaEvento evento, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JornadaEvento>> GetByPetAsync(int petId, int limite, CancellationToken cancellationToken = default);
}
