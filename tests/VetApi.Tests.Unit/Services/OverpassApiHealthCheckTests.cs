using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using VetApi.Services.Health;
using Xunit;

namespace VetApi.Tests.Unit.Services;

public class OverpassApiHealthCheckTests
{
    private static IHttpClientFactory CriarFactory(FakeHttpMessageHandler handler)
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler));
        return factoryMock.Object;
    }

    [Fact]
    public async Task CheckHealthAsync_OverpassApiRespondeOk_RetornaHealthy()
    {
        var handler = new FakeHttpMessageHandler("OK", HttpStatusCode.OK);
        var healthCheck = new OverpassApiHealthCheck(CriarFactory(handler), Mock.Of<ILogger<OverpassApiHealthCheck>>());

        var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        resultado.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_OverpassApiRetornaErroHttp_RetornaDegraded()
    {
        var handler = new FakeHttpMessageHandler("indisponível", HttpStatusCode.ServiceUnavailable);
        var healthCheck = new OverpassApiHealthCheck(CriarFactory(handler), Mock.Of<ILogger<OverpassApiHealthCheck>>());

        var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        resultado.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public async Task CheckHealthAsync_ExcecaoAoChamarApi_RetornaDegradedComExcecaoRegistrada()
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Falha simulada de rede"));

        var healthCheck = new OverpassApiHealthCheck(factoryMock.Object, Mock.Of<ILogger<OverpassApiHealthCheck>>());

        var resultado = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        resultado.Status.Should().Be(HealthStatus.Degraded);
        resultado.Exception.Should().NotBeNull();
    }
}
