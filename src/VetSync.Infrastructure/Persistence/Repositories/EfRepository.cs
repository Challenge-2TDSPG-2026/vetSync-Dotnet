using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VetSync.Domain.Repositories;

namespace VetSync.Infrastructure.Persistence.Repositories;

/// <summary>Implementação base (padrão Repository) sobre o EF Core.</summary>
public abstract class EfRepository<T> : IRepository<T> where T : class
{
    protected EfRepository(AppDbContext context) => Context = context;

    protected AppDbContext Context { get; }
    protected DbSet<T> Set => Context.Set<T>();

    public virtual Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Set.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        => Set.AnyAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Set.AddAsync(entity, cancellationToken);

    public void Remove(T entity) => Set.Remove(entity);

    /// <summary>Ordena por uma chave e adiciona o Id como desempate (paginação estável).</summary>
    protected static IOrderedQueryable<T> Ordenar<TKey>(
        IQueryable<T> query, Expression<Func<T, TKey>> chave, bool descendente)
    {
        var ordenada = descendente ? query.OrderByDescending(chave) : query.OrderBy(chave);
        return ordenada.ThenBy(e => EF.Property<int>(e, "Id"));
    }

    protected static async Task<PagedList<T>> PaginarAsync(
        IQueryable<T> query, PageRequest page, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var itens = await query.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PagedList<T>(itens, total);
    }
}
