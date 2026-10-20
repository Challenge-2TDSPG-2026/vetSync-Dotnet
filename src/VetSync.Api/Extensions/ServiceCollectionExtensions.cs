using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using VetSync.Infrastructure.Mongo;
using VetSync.Infrastructure.Persistence;
using VetSync.Infrastructure.Security;
using VetSync.Infrastructure.Services.Health;

namespace VetSync.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Autenticação JWT Bearer. Os parâmetros vêm de <see cref="JwtSettings"/> (resolvidos em tempo de execução).</summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "VetSync API - FIAP",
                Version = "v1",
                Description = "API da jornada contínua de cuidado do pet (Clyvo Vet). " +
                              "Autentique-se em /api/auth/login e use o botão **Authorize** com o token JWT.",
                Contact = new OpenApiContact { Name = "FIAP", Url = new Uri("https://www.fiap.com.br") }
            });

            // XML comments de todos os assemblies VetSync.* (API + DTOs da camada Application)
            foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "VetSync.*.xml"))
                c.IncludeXmlComments(xml);

            c.EnableAnnotations();
            c.UseInlineDefinitionsForEnums();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe somente o token JWT (o prefixo 'Bearer' é adicionado automaticamente).",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API em execução"), tags: new[] { "live" })
            .AddDbContextCheck<AppDbContext>(
                name: "oracle-database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "db", "oracle" })
            .AddCheck<MongoHealthCheck>(
                name: "mongodb",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "db", "mongodb" })
            .AddCheck<OverpassApiHealthCheck>(
                name: "overpass-api",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "external" });

        return services;
    }

    public static IServiceCollection AddObservability(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: "VetSync.API", serviceVersion: "1.0.0"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health")
                                             && !ctx.Request.Path.StartsWithSegments("/metrics");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("VetSync.API")
                .AddConsoleExporter()
                .AddPrometheusExporter());

        return services;
    }
}
