using Microsoft.EntityFrameworkCore;
using VetSync.Domain.Entities;

namespace VetSync.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Vacinacao> Vacinacoes => Set<Vacinacao>();
    public DbSet<Exame> Exames => Set<Exame>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
