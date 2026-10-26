using Microsoft.Extensions.DependencyInjection;
using VetSync.Application.Abstractions;
using VetSync.Application.Services;

namespace VetSync.Application;

public static class DependencyInjection
{
    /// <summary>Registra os serviços de aplicação (casos de uso).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITutorService, TutorService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IConsultaService, ConsultaService>();
        services.AddScoped<IVacinacaoService, VacinacaoService>();
        services.AddScoped<IExameService, ExameService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClinicaService, ClinicaService>();
        services.AddScoped<IJornadaEventPublisher, JornadaEventPublisher>();
        return services;
    }
}
