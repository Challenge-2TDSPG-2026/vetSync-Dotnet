using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetSync.Application.Abstractions;
using VetSync.Domain.Exceptions;

namespace VetSync.Api.Middleware;

/// <summary>
/// Ponto único de tratamento de exceções: converte exceções de domínio/aplicação em respostas
/// RFC 7807 (ProblemDetails) com o status HTTP correto, e esconde detalhes internos em erros 500.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = Mapear(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("{Title}: {Detail} ({Method} {Path})",
                title, detail, httpContext.Request.Method, httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            problem.Extensions["correlationId"] = correlationId;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int Status, string Title, string Detail) Mapear(Exception exception) => exception switch
    {
        NotFoundException e => (StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
        ConflictException e => (StatusCodes.Status409Conflict, "Conflito de dados", e.Message),
        AuthenticationFailedException e => (StatusCodes.Status401Unauthorized, "Não autenticado", e.Message),
        BusinessRuleException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
        BadHttpRequestException e => (StatusCodes.Status400BadRequest, "Requisição inválida", e.Message),
        VeterinaryClinicSearchException => (StatusCodes.Status503ServiceUnavailable, "Serviço indisponível",
            "Não foi possível buscar clínicas reais no momento. Tente novamente em instantes."),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor",
            "Ocorreu um erro inesperado. Informe o correlationId ao suporte.")
    };
}
