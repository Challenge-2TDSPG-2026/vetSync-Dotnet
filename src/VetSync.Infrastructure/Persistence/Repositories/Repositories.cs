using Microsoft.EntityFrameworkCore;
using VetSync.Domain.Entities;
using VetSync.Domain.Repositories;

namespace VetSync.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public UnitOfWork(AppDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

public class TutorRepository : EfRepository<Tutor>, ITutorRepository
{
    public TutorRepository(AppDbContext context) : base(context) { }

    public Task<PagedList<Tutor>> SearchAsync(TutorFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Tutor> query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Nome))
        {
            var nome = filter.Nome.Trim().ToLower();
            query = query.Where(t => t.Nome.ToLower().Contains(nome));
        }
        if (!string.IsNullOrWhiteSpace(filter.Email))
        {
            var email = filter.Email.Trim().ToLower();
            query = query.Where(t => t.Email.ToLower() == email);
        }
        if (filter.Ativo.HasValue)
            query = query.Where(t => t.Ativo == filter.Ativo.Value);

        var ordenada = page.SortBy?.ToLowerInvariant() switch
        {
            "id" => Ordenar(query, t => t.Id, page.Descending),
            "email" => Ordenar(query, t => t.Email, page.Descending),
            "criadoem" => Ordenar(query, t => t.CriadoEm, page.Descending),
            _ => Ordenar(query, t => t.Nome, page.Descending)
        };

        return PaginarAsync(ordenada, page, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, int? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLower();
        return Set.AnyAsync(t => t.Email.ToLower() == normalizado && (ignorarId == null || t.Id != ignorarId),
            cancellationToken);
    }

    public Task<bool> CpfExistsAsync(string cpf, int? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var normalizado = cpf.Trim();
        return Set.AnyAsync(t => t.Cpf == normalizado && (ignorarId == null || t.Id != ignorarId),
            cancellationToken);
    }

    public Task<bool> HasPetsAsync(int tutorId, CancellationToken cancellationToken = default)
        => Context.Pets.AnyAsync(p => p.TutorId == tutorId, cancellationToken);
}

public class PetRepository : EfRepository<Pet>, IPetRepository
{
    public PetRepository(AppDbContext context) : base(context) { }

    public override Task<Pet?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Pet?> GetWithJornadaAsync(int id, CancellationToken cancellationToken = default)
        => Set.AsNoTracking()
            .Include(p => p.Tutor)
            .Include(p => p.Consultas)
            .Include(p => p.Vacinacoes)
            .Include(p => p.Exames)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<PagedList<Pet>> SearchAsync(PetFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Pet> query = Set.AsNoTracking().Include(p => p.Tutor);

        if (!string.IsNullOrWhiteSpace(filter.Nome))
        {
            var nome = filter.Nome.Trim().ToLower();
            query = query.Where(p => p.Nome.ToLower().Contains(nome));
        }
        if (!string.IsNullOrWhiteSpace(filter.Especie))
        {
            var especie = filter.Especie.Trim().ToLower();
            query = query.Where(p => p.Especie.ToLower() == especie);
        }
        if (!string.IsNullOrWhiteSpace(filter.Raca))
        {
            var raca = filter.Raca.Trim().ToLower();
            query = query.Where(p => p.Raca != null && p.Raca.ToLower().Contains(raca));
        }
        if (filter.Ativo.HasValue)
            query = query.Where(p => p.Ativo == filter.Ativo.Value);
        if (filter.TutorId.HasValue)
            query = query.Where(p => p.TutorId == filter.TutorId.Value);

        var ordenada = page.SortBy?.ToLowerInvariant() switch
        {
            "id" => Ordenar(query, p => p.Id, page.Descending),
            "especie" => Ordenar(query, p => p.Especie, page.Descending),
            "raca" => Ordenar(query, p => p.Raca, page.Descending),
            "criadoem" => Ordenar(query, p => p.CriadoEm, page.Descending),
            _ => Ordenar(query, p => p.Nome, page.Descending)
        };

        return PaginarAsync(ordenada, page, cancellationToken);
    }
}

public class ConsultaRepository : EfRepository<Consulta>, IConsultaRepository
{
    public ConsultaRepository(AppDbContext context) : base(context) { }

    public override Task<Consulta?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(c => c.Pet).ThenInclude(p => p.Tutor)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<PagedList<Consulta>> SearchAsync(ConsultaFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Consulta> query = Set.AsNoTracking().Include(c => c.Pet).ThenInclude(p => p.Tutor);

        if (filter.PetId.HasValue)
            query = query.Where(c => c.PetId == filter.PetId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(c => c.Status == status);
        }
        if (filter.De.HasValue)
            query = query.Where(c => c.DataConsulta >= filter.De.Value);
        if (filter.Ate.HasValue)
            query = query.Where(c => c.DataConsulta <= filter.Ate.Value);

        var ordenada = page.SortBy?.ToLowerInvariant() switch
        {
            "id" => Ordenar(query, c => c.Id, page.Descending),
            "status" => Ordenar(query, c => c.Status, page.Descending),
            "veterinario" => Ordenar(query, c => c.Veterinario, page.Descending),
            _ => Ordenar(query, c => c.DataConsulta, page.Descending)
        };

        return PaginarAsync(ordenada, page, cancellationToken);
    }
}

public class VacinacaoRepository : EfRepository<Vacinacao>, IVacinacaoRepository
{
    public VacinacaoRepository(AppDbContext context) : base(context) { }

    public override Task<Vacinacao?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(v => v.Pet).FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<PagedList<Vacinacao>> SearchAsync(VacinacaoFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Vacinacao> query = Set.AsNoTracking().Include(v => v.Pet);

        if (filter.PetId.HasValue)
            query = query.Where(v => v.PetId == filter.PetId.Value);
        if (filter.ProximaDoseAte.HasValue)
            query = query.Where(v => v.ProximaDose != null && v.ProximaDose <= filter.ProximaDoseAte.Value);

        var ordenada = page.SortBy?.ToLowerInvariant() switch
        {
            "id" => Ordenar(query, v => v.Id, page.Descending),
            "proximadose" => Ordenar(query, v => v.ProximaDose, page.Descending),
            "vacina" => Ordenar(query, v => v.Vacina, page.Descending),
            _ => Ordenar(query, v => v.DataAplicacao, page.Descending)
        };

        return PaginarAsync(ordenada, page, cancellationToken);
    }
}

public class ExameRepository : EfRepository<Exame>, IExameRepository
{
    public ExameRepository(AppDbContext context) : base(context) { }

    public override Task<Exame?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Set.Include(e => e.Pet).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<PagedList<Exame>> SearchAsync(ExameFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Exame> query = Set.AsNoTracking().Include(e => e.Pet);

        if (filter.PetId.HasValue)
            query = query.Where(e => e.PetId == filter.PetId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Tipo))
        {
            var tipo = filter.Tipo.Trim().ToLower();
            query = query.Where(e => e.TipoExame.ToLower().Contains(tipo));
        }

        var ordenada = page.SortBy?.ToLowerInvariant() switch
        {
            "id" => Ordenar(query, e => e.Id, page.Descending),
            "tipoexame" => Ordenar(query, e => e.TipoExame, page.Descending),
            _ => Ordenar(query, e => e.DataExame, page.Descending)
        };

        return PaginarAsync(ordenada, page, cancellationToken);
    }
}

public class UsuarioRepository : EfRepository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(AppDbContext context) : base(context) { }

    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLower();
        return Set.FirstOrDefaultAsync(u => u.Email == normalizado, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizado = email.Trim().ToLower();
        return Set.AnyAsync(u => u.Email == normalizado, cancellationToken);
    }
}
