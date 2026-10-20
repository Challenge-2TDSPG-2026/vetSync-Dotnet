using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VetSync.Application.Abstractions;
using VetSync.Domain.Repositories;
using VetSync.Infrastructure.Persistence;

namespace VetSync.Tests.Integration.Fixtures;

/// <summary>
/// Sobe a API real em memória: EF InMemory no lugar do Oracle, repositório em memória no lugar do
/// MongoDB e busca de clínicas falsa. Todo o resto (middleware, JWT, filtros, serviços) é o de produção.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@vetsync.test";
    public const string AdminPassword = "Admin@123";

    public string DatabaseName { get; } = Guid.NewGuid().ToString();
    public InMemoryJornadaEventoRepository Jornada { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "integration-tests-secret-key-0123456789-abcdefghijkl",
                ["Jwt:Issuer"] = "VetSync.Tests",
                ["Jwt:Audience"] = "VetSync.Tests.Clients",
                ["Jwt:ExpirationMinutes"] = "30",
                ["Seed:AdminEmail"] = AdminEmail,
                ["Seed:AdminPassword"] = AdminPassword,
                ["Mongo:EnsureIndexes"] = "false",
                ["Mongo:ServerSelectionTimeoutSeconds"] = "1"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptorsParaRemover = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(AppDbContext)
                         || d.ServiceType == typeof(IJornadaEventoRepository)
                         || d.ServiceType == typeof(IVeterinaryClinicSearchService)
                         || (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();

            foreach (var descriptor in descriptorsParaRemover)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(DatabaseName));
            services.AddSingleton<IJornadaEventoRepository>(Jornada);
            services.AddSingleton<IVeterinaryClinicSearchService, FakeVeterinaryClinicSearchService>();
        });
    }
}
