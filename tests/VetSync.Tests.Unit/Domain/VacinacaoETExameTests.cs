using FluentAssertions;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using Xunit;

namespace VetSync.Tests.Unit.Domain;

public class VacinacaoTests
{
    [Fact]
    public void Validar_ProximaDoseAnteriorAAplicacao_LancaBusinessRuleException()
    {
        // Arrange
        var vacina = new Vacinacao
        {
            DataAplicacao = new DateTime(2026, 5, 10),
            ProximaDose = new DateTime(2026, 5, 9)
        };

        // Act
        var act = () => vacina.Validar();

        // Assert
        act.Should().Throw<BusinessRuleException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("2026-05-10")]
    [InlineData("2027-05-10")]
    public void Validar_ProximaDoseNulaOuIgualOuPosterior_NaoLancaExcecao(string? proxima)
    {
        // Arrange
        var vacina = new Vacinacao
        {
            DataAplicacao = new DateTime(2026, 5, 10),
            ProximaDose = proxima is null ? null : DateTime.Parse(proxima)
        };

        // Act
        var act = () => vacina.Validar();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("2026-10-18", true)]
    [InlineData("2026-10-19", false)]
    [InlineData("2026-12-01", false)]
    public void ProximaDoseVencida_ComparaComDataDeReferencia(string proxima, bool esperado)
    {
        // Arrange
        var vacina = new Vacinacao { ProximaDose = DateTime.Parse(proxima) };

        // Act
        var vencida = vacina.ProximaDoseVencida(new DateTime(2026, 10, 19));

        // Assert
        vencida.Should().Be(esperado);
    }

    [Fact]
    public void ProximaDoseVencida_SemProximaDose_RetornaFalse()
    {
        // Arrange
        var vacina = new Vacinacao { ProximaDose = null };

        // Act
        var vencida = vacina.ProximaDoseVencida(DateTime.UtcNow);

        // Assert
        vencida.Should().BeFalse();
    }
}

public class ExameTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Hemograma normal", true)]
    public void PossuiResultado_DependeDoTextoInformado(string? resultado, bool esperado)
    {
        // Arrange
        var exame = new Exame { Resultado = resultado };

        // Act
        var possui = exame.PossuiResultado;

        // Assert
        possui.Should().Be(esperado);
    }

    [Fact]
    public void Atualizar_ComResultado_PreencheCamposEDataDeAtualizacao()
    {
        // Arrange
        var exame = new Exame { TipoExame = "Sangue" };

        // Act
        exame.Atualizar("Hemograma", new DateTime(2026, 10, 1), "OK", "LabVet", "Dr. Bruno");

        // Assert
        exame.TipoExame.Should().Be("Hemograma");
        exame.Resultado.Should().Be("OK");
        exame.AtualizadoEm.Should().NotBeNull();
    }
}
