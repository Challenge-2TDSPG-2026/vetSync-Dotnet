using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Domain.Entities;
using VetSync.Infrastructure.Mongo;

namespace VetSync.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>
    /// Aplica as migrations (com retentativas enquanto o Oracle sobe), cria o administrador
    /// inicial (se configurado em Seed:*) e garante os índices do MongoDB (best effort).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var db = sp.GetRequiredService<AppDbContext>();

        if (db.Database.IsRelational())
        {
            var tentativas = 10;
            while (true)
            {
                try
                {
                    await db.Database.MigrateAsync(ct);
                    logger.LogInformation("Migrations aplicadas com sucesso.");
                    break;
                }
                catch (Exception ex) when (--tentativas > 0 && ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Oracle ainda não disponível. Tentativas restantes: {Tentativas}", tentativas);
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                }
            }
        }
        else
        {
            await db.Database.EnsureCreatedAsync(ct);
        }

        await SeedAdminAsync(sp, logger, ct);
        await EnsureMongoIndexesAsync(sp, logger, ct);
    }

    private static async Task SeedAdminAsync(IServiceProvider sp, ILogger logger, CancellationToken ct)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var email = configuration["Seed:AdminEmail"];
        var senha = configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return;

        var db = sp.GetRequiredService<AppDbContext>();
        var normalizado = email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == normalizado, ct))
            return;

        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var nome = configuration["Seed:AdminName"] ?? "Administrador";
        db.Usuarios.Add(Usuario.Criar(nome, normalizado, hasher.Hash(senha), Roles.Admin));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Usuário administrador inicial criado: {Email}", normalizado);
    }

    private static async Task EnsureMongoIndexesAsync(IServiceProvider sp, ILogger logger, CancellationToken ct)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Mongo:EnsureIndexes", true))
            return;

        try
        {
            var repo = sp.GetRequiredService<MongoJornadaEventoRepository>();
            await repo.EnsureIndexesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "MongoDB indisponível: índices da jornada não foram criados agora.");
        }
    }
}
