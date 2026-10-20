using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VetSync.Application.Abstractions;
using VetSync.Domain.Repositories;
using VetSync.Infrastructure.Mongo;
using VetSync.Infrastructure.Persistence;
using VetSync.Infrastructure.Persistence.Repositories;
using VetSync.Infrastructure.Security;
using VetSync.Infrastructure.Services;

namespace VetSync.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra persistência (Oracle + MongoDB), segurança e integrações externas.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ---------- Relacional (Oracle via EF Core) ----------
        services.AddDbContext<AppDbContext>(options =>
            options.UseOracle(configuration.GetConnectionString("OracleConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITutorRepository, TutorRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IConsultaRepository, ConsultaRepository>();
        services.AddScoped<IVacinacaoRepository, VacinacaoRepository>();
        services.AddScoped<IExameRepository, ExameRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // ---------- NoSQL (MongoDB) ----------
        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(settings.ServerSelectionTimeoutSeconds);
            return new MongoClient(clientSettings);
        });
        services.AddScoped<MongoJornadaEventoRepository>();
        services.AddScoped<IJornadaEventoRepository>(sp => sp.GetRequiredService<MongoJornadaEventoRepository>());

        // ---------- Segurança ----------
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Key) && System.Text.Encoding.UTF8.GetByteCount(s.Key) >= 32,
                "Jwt:Key é obrigatório e deve ter no mínimo 32 caracteres (use variável de ambiente Jwt__Key ou user-secrets).")
            .ValidateOnStart();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        // ---------- Integrações externas ----------
        services.AddSingleton<IGeoLocationService, GeoLocationService>();

        services.AddHttpClient<IVeterinaryClinicSearchService, OverpassVeterinaryClinicSearchService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(25);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("VetSyncApi/1.0 (FIAP Challenge; contato@vetsync.example)");
        });

        services.AddHttpClient("OverpassHealthCheck", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("VetSyncApi/1.0 (FIAP Challenge; HealthCheck)");
        });

        return services;
    }
}
