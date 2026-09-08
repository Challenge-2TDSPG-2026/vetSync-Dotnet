using FluentAssertions;
using VetApi.Services;
using Xunit;

namespace VetApi.Tests.Unit.Services;

[Collection("GeoLocationService Collection")]
public class GeoLocationServiceTests
{
    private readonly IGeoLocationService _sut;

    public GeoLocationServiceTests(GeoLocationServiceFixture fixture)
    {
        _sut = fixture.Service;
    }

    [Fact]
    public void CalcularDistanciaKm_MesmoPonto_RetornaZero()
    {
        const double latitude = -23.561684;
        const double longitude = -46.655981;

        var distancia = _sut.CalcularDistanciaKm(latitude, longitude, latitude, longitude);

        distancia.Should().Be(0);
    }

    [Fact]
    public void CalcularDistanciaKm_PaulistaEPinheiros_RetornaDistanciaAproximadaEsperada()
    {
        const double latOrigem = -23.561684;
        const double lonOrigem = -46.655981;
        const double latDestino = -23.566389;
        const double lonDestino = -46.686389;

        var distancia = _sut.CalcularDistanciaKm(latOrigem, lonOrigem, latDestino, lonDestino);

        distancia.Should().BeApproximately(3.1, 0.5);
    }

    [Fact]
    public void CalcularDistanciaKm_PontosDistantesForaDoRaioDe10Km_RetornaDistanciaMaiorQue10()
    {
        const double latOrigem = -23.561684;
        const double lonOrigem = -46.655981;
        const double latDestino = -23.462222;
        const double lonDestino = -46.533333;

        var distancia = _sut.CalcularDistanciaKm(latOrigem, lonOrigem, latDestino, lonDestino);

        distancia.Should().BeGreaterThan(10);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(-90, -180, true)]
    [InlineData(90, 180, true)]
    [InlineData(-23.5, -46.6, true)]
    public void CoordenadasValidas_CoordenadasDentroDoLimite_RetornaTrue(double latitude, double longitude, bool esperado)
    {

        var resultado = _sut.CoordenadasValidas(latitude, longitude);

        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void CoordenadasValidas_CoordenadasForaDoLimite_RetornaFalse(double latitude, double longitude)
    {

        var resultado = _sut.CoordenadasValidas(latitude, longitude);

        resultado.Should().BeFalse();
    }
}
