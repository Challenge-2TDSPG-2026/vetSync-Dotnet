using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using VetSync.Api.Middleware;
using VetSync.Application.Abstractions;
using VetSync.Domain.Exceptions;
using Xunit;

namespace VetSync.Tests.Unit.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut = new(NullLogger<GlobalExceptionHandler>.Instance);

    private static DefaultHttpContext NovoContexto()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        ctx.Request.Path = "/api/pets/9";
        ctx.Items["CorrelationId"] = "abc123";
        return ctx;
    }

    private static JsonElement LerCorpo(DefaultHttpContext ctx)
    {
        ctx.Response.Body.Position = 0;
        return JsonDocument.Parse(ctx.Response.Body).RootElement;
    }

    public static IEnumerable<object[]> Casos() => new[]
    {
        new object[] { new NotFoundException("não achou"), 404 },
        new object[] { new ConflictException("duplicado"), 409 },
        new object[] { new BusinessRuleException("regra"), 400 },
        new object[] { new AuthenticationFailedException("credenciais"), 401 },
        new object[] { new VeterinaryClinicSearchException("falha externa"), 503 },
        new object[] { new InvalidOperationException("bug"), 500 }
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public async Task TryHandleAsync_ExcecaoConhecida_MapeiaParaOStatusHttpCorreto(Exception excecao, int statusEsperado)
    {
        // Arrange
        var ctx = NovoContexto();

        // Act
        var tratada = await _sut.TryHandleAsync(ctx, excecao, CancellationToken.None);

        // Assert
        tratada.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(statusEsperado);
        LerCorpo(ctx).GetProperty("status").GetInt32().Should().Be(statusEsperado);
    }

    [Fact]
    public async Task TryHandleAsync_ErroInesperado_NaoVazaMensagemInternaEIncluiCorrelationId()
    {
        // Arrange
        var ctx = NovoContexto();

        // Act
        await _sut.TryHandleAsync(ctx, new InvalidOperationException("senha do banco = 123"), CancellationToken.None);
        var corpo = LerCorpo(ctx);

        // Assert
        corpo.GetProperty("detail").GetString().Should().NotContain("senha do banco");
        corpo.GetProperty("correlationId").GetString().Should().Be("abc123");
        corpo.GetProperty("instance").GetString().Should().Be("/api/pets/9");
    }

    [Fact]
    public async Task TryHandleAsync_ExcecaoDeNegocio_ExpoeAMensagemNoDetail()
    {
        // Arrange
        var ctx = NovoContexto();

        // Act
        await _sut.TryHandleAsync(ctx, new ConflictException("E-mail já cadastrado"), CancellationToken.None);

        // Assert
        LerCorpo(ctx).GetProperty("detail").GetString().Should().Be("E-mail já cadastrado");
    }
}
