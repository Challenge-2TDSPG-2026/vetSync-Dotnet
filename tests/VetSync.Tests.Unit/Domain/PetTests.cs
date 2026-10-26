using FluentAssertions;
using VetSync.Domain.Entities;
using Xunit;

namespace VetSync.Tests.Unit.Domain;

public class PetTests
{
    [Fact]
    public void CalcularIdadeEmAnos_SemDataNascimento_RetornaNulo()
    {
        // Arrange
        var pet = new Pet { DataNascimento = null };

        // Act
        var idade = pet.CalcularIdadeEmAnos(new DateTime(2026, 10, 19));

        // Assert
        idade.Should().BeNull();
    }

    [Theory]
    [InlineData("2020-10-19", "2026-10-19", 6)]   // aniversário no dia
    [InlineData("2020-10-20", "2026-10-19", 5)]   // falta 1 dia para completar
    [InlineData("2026-10-19", "2026-10-19", 0)]   // nasceu hoje
    public void CalcularIdadeEmAnos_ComDataNascimento_RetornaIdadeCompleta(string nascimento, string referencia, int esperado)
    {
        // Arrange
        var pet = new Pet { DataNascimento = DateTime.Parse(nascimento) };

        // Act
        var idade = pet.CalcularIdadeEmAnos(DateTime.Parse(referencia));

        // Assert
        idade.Should().Be(esperado);
    }

    [Fact]
    public void CalcularIdadeEmAnos_NascimentoNoFuturo_RetornaNulo()
    {
        // Arrange
        var pet = new Pet { DataNascimento = new DateTime(2030, 1, 1) };

        // Act
        var idade = pet.CalcularIdadeEmAnos(new DateTime(2026, 10, 19));

        // Assert
        idade.Should().BeNull();
    }

    [Fact]
    public void Atualizar_ComNovosDados_AlteraCamposEMarcaAtualizadoEm()
    {
        // Arrange
        var pet = new Pet { Nome = "Rex", Especie = "Cão", Ativo = true };

        // Act
        pet.Atualizar("Thor", "Cão", "Labrador", "Macho", null, 30.5m, "Preto", false);

        // Assert
        pet.Nome.Should().Be("Thor");
        pet.Raca.Should().Be("Labrador");
        pet.Peso.Should().Be(30.5m);
        pet.Ativo.Should().BeFalse();
        pet.AtualizadoEm.Should().NotBeNull();
    }
}
