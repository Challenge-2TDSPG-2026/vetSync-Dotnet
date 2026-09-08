using System.Reflection;
using System.Text.Json;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using VetApi.Data;
using VetApi.Mappings;
using VetApi.Services;
using VetApi.Services.Health;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando VetSync API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Application", "VetSync.API")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/vetsync-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddControllers();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddSingleton<IGeoLocationService, GeoLocationService>();

    builder.Services.AddHttpClient<IVeterinaryClinicSearchService, OverpassVeterinaryClinicSearchService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(25);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VetSyncApi/1.0 (FIAP Challenge; contato@vetsync.example)");
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "VetSync API - FIAP",
            Version = "v1",
            Description = @"
## API RESTful para Clínica Veterinária + Geolocalização

Desenvolvida com **ASP.NET Core 8**, **Oracle Database** e **Entity Framework Core**.

### Entidades
- **Tutores** — cadastro dos responsáveis pelos pets
- **Pets** — cadastro dos animais
- **Consultas** — agendamento e registro de consultas
- **Vacinações** — histórico de vacinas aplicadas
- **Exames** — exames solicitados e resultados

### Jornada do Pet
Use `GET /api/pets/{id}/jornada` para ver o histórico completo de um pet.

### Clínicas Veterinárias Próximas (Geolocalização real)
Use `GET /api/clinicas/proximas?latitude=..&longitude=..&raioKm=10` para listar
clínicas veterinárias **reais** (dados públicos do OpenStreetMap, sem nada
mockado) num raio de até 50km da localização informada, ordenadas pela
distância (fórmula de Haversine). Também há uma página de demonstração em
`/buscar-clinicas.html` que pede a localização do navegador (GPS) e chama
esse endpoint automaticamente.

### Observabilidade
- `GET /health` — status geral (JSON detalhado)
- `GET /health/live` — liveness (a API está de pé?)
- `GET /health/ready` — readiness (dependências, ex. Oracle, disponíveis?)
        ",
            Contact = new OpenApiContact { Name = "FIAP", Url = new Uri("https://www.fiap.com.br") }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);

        c.EnableAnnotations();
        c.UseInlineDefinitionsForEnums();
    });

    builder.Services.AddHttpClient("OverpassHealthCheck", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(5);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VetSyncApi/1.0 (FIAP Challenge; HealthCheck)");
    });

    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API em execução"), tags: new[] { "live" })
        .AddDbContextCheck<AppDbContext>(
            name: "oracle-database",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "db", "oracle" })
        .AddCheck<OverpassApiHealthCheck>(
            name: "overpass-api",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ready", "external" });

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: "VetSync.API", serviceVersion: "1.0.0"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("VetSync.API")
            .AddConsoleExporter());

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (db.Database.IsRelational())
        {
            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    db.Database.Migrate();
                    logger.LogInformation("Migrations aplicadas com sucesso.");
                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    logger.LogWarning(ex, "Oracle ainda não disponível. Tentativas restantes: {Retries}", retries);
                    Thread.Sleep(10000);
                }
            }
        }
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VetSync API v1");
        c.RoutePrefix = string.Empty;
        c.DefaultModelsExpandDepth(2);
        c.DisplayRequestDuration();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseAuthorization();
    app.MapControllers();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação VetSync API falhou ao iniciar.");
}
finally
{
    Log.CloseAndFlush();
}

static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var payload = new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            durationMs = e.Value.Duration.TotalMilliseconds,
            description = e.Value.Description,
            error = e.Value.Exception?.Message
        })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
}

public partial class Program { }
