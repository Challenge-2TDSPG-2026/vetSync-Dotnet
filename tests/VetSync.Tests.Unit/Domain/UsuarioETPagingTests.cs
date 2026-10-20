using FluentAssertions;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Domain;

public class UsuarioTests
{
    [Fact]
    public void Criar_DadosValidos_NormalizaEmailEDefinePerfil()
    {
        // Arrange / Act
        var usuario = Usuario.Criar("  Maria ", "  MARIA@Vet.com ", "hash", Roles.Usuario);

        // Assert
        usuario.Nome.Should().Be("Maria");
        usuario.Email.Should().Be("maria@vet.com");
        usuario.Role.Should().Be(Roles.Usuario);
        usuario.Ativo.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "a@a.com", "hash", "Usuario")]
    [InlineData("Maria", "", "hash", "Usuario")]
    [InlineData("Maria", "a@a.com", "", "Usuario")]
    [InlineData("Maria", "a@a.com", "hash", "SuperAdmin")]
    public void Criar_DadosInvalidos_LancaBusinessRuleException(string nome, string email, string hash, string role)
    {
        // Arrange / Act
        var act = () => Usuario.Criar(nome, email, hash, role);

        // Assert
        act.Should().Throw<BusinessRuleException>();
    }
}

public class PageRequestTests
{
    [Fact]
    public void Page_ValorMenorQueUm_AjustaParaUm()
    {
        // Arrange / Act
        var page = new PageRequest { Page = -5 };

        // Assert
        page.Page.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-3, 10)]
    [InlineData(25, 25)]
    [InlineData(5000, 100)]
    public void PageSize_ValorForaDoIntervalo_EhAjustado(int informado, int esperado)
    {
        // Arrange / Act
        var page = new PageRequest { PageSize = informado };

        // Assert
        page.PageSize.Should().Be(esperado);
    }

    [Fact]
    public void Skip_PaginaTresComVinteItens_RetornaQuarenta()
    {
        // Arrange / Act
        var page = new PageRequest { Page = 3, PageSize = 20 };

        // Assert
        page.Skip.Should().Be(40);
    }
}
