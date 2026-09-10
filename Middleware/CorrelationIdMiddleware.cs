using Serilog.Context;

namespace VetApi.Middleware;

/// <summary>
/// Garante que toda requisição HTTP tenha um Correlation ID: reaproveita o valor recebido
/// no header "X-Correlation-ID" ou gera um novo (GUID) quando ausente. O valor é devolvido
/// no header de resposta e injetado no LogContext do Serilog, permitindo correlacionar
/// todas as linhas de log geradas durante o processamento de uma mesma requisição.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Items["CorrelationId"] = correlationId;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
                context.Response.Headers[HeaderName] = correlationId;

            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var valores))
        {
            var valor = valores.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(valor))
                return valor!;
        }

        return Guid.NewGuid().ToString("N");
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>Registra o middleware de Correlation ID no pipeline HTTP.</summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}