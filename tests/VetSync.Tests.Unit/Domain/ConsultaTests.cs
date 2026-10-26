using FluentAssertions;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using Xunit;

namespace VetSync.Tests.Unit.Domain;

public class ConsultaTests
{
    [Theory]
    [InlineData("Agendada")]
    [InlineData("Realizada")]
    [InlineData("Cancelada")]
    [InlineData("Em andamento")]
    public void StatusEhValido_StatusConhecido_RetornaTrue(string status)
    {
        // Arrange / Act
        var valido = Consulta.StatusEhValido(status);

        // Assert
        valido.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Inexistente")]
    public void StatusEhValido_StatusDesconhecido_RetornaFalse(string? status)
    {
        // Arrange / Act
        var valido = Consulta.StatusEhValido(status);

        // Assert
        valido.Should().BeFalse();
    }

    [Fact]
    public void Atualizar_StatusValido_AtualizaCamposEDataDeAtualizacao()
    {
        // Arrange
        var consulta = new Consulta { Veterinario = "Dra. Ana" };
        var data = new DateTime(2026, 11, 1, 10, 0, 0);

        // Act
        consulta.Atualizar("Dr. Bruno", data, "Rotina", "Saudável", "Nenhuma", Consulta.StatusRealizada);

        // Assert
        consulta.Veterinario.Should().Be("Dr. Bruno");
        consulta.DataConsulta.Should().Be(data);
        consulta.Status.Should().Be("Realizada");
        consulta.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Atualizar_StatusInvalido_LancaBusinessRuleExceptionESemAlterarDados()
    {
        // Arrange
        var consulta = new Consulta { Veterinario = "Dra. Ana", Status = Consulta.StatusAgendada };

        // Act
        var act = () => consulta.Atualizar("Dr. Bruno", DateTime.UtcNow, null, null, null, "Qualquer");

        // Assert
        act.Should().Throw<BusinessRuleException>().WithMessage("*Status inválido*");
        consulta.Veterinario.Should().Be("Dra. Ana");
        consulta.Status.Should().Be(Consulta.StatusAgendada);
    }
}
