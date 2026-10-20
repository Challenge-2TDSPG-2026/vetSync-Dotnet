using FluentAssertions;
using VetSync.Application.Common;
using VetSync.Domain.Exceptions;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class PageQueryTests
{
    private static readonly string[] Campos = { "id", "nome", "criadoEm" };

    [Fact]
    public void ToPageRequest_SemOrdenacao_UsaCampoEDirecaoPadrao()
    {
        // Arrange
        var query = new PageQuery { Page = 2, PageSize = 5 };

        // Act
        var page = query.ToPageRequest(Campos, "nome");

        // Assert
        page.SortBy.Should().Be("nome");
        page.Descending.Should().BeFalse();
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(5);
    }

    [Fact]
    public void ToPageRequest_SemOrdenacaoComPadraoDescendente_UsaDescendente()
    {
        // Arrange
        var query = new PageQuery();

        // Act
        var page = query.ToPageRequest(Campos, "criadoEm", descPadrao: true);

        // Assert
        page.Descending.Should().BeTrue();
    }

    [Fact]
    public void ToPageRequest_CampoEmCaixaDiferente_NormalizaParaONomeOficial()
    {
        // Arrange
        var query = new PageQuery { SortBy = "CRIADOEM", SortDir = "DESC" };

        // Act
        var page = query.ToPageRequest(Campos, "nome");

        // Assert
        page.SortBy.Should().Be("criadoEm");
        page.Descending.Should().BeTrue();
    }

    [Fact]
    public void ToPageRequest_CampoNaoPermitido_LancaBusinessRuleException()
    {
        // Arrange
        var query = new PageQuery { SortBy = "senhaHash" };

        // Act
        var act = () => query.ToPageRequest(Campos, "nome");

        // Assert
        act.Should().Throw<BusinessRuleException>().WithMessage("*ordenação inválido*");
    }

    [Fact]
    public void ToPageRequest_DirecaoInvalida_LancaBusinessRuleException()
    {
        // Arrange
        var query = new PageQuery { SortBy = "nome", SortDir = "sideways" };

        // Act
        var act = () => query.ToPageRequest(Campos, "nome");

        // Assert
        act.Should().Throw<BusinessRuleException>().WithMessage("*Direção*");
    }
}

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0, false, false)]
    [InlineData(25, 10, 3, false, true)]
    public void Create_CalculaTotalDePaginasENavegacao(int total, int pageSize, int totalPages, bool hasPrev, bool hasNext)
    {
        // Arrange
        var page = new VetSync.Domain.Repositories.PageRequest { Page = 1, PageSize = pageSize };

        // Act
        var result = PagedResult<string>.Create(new List<string>(), total, page);

        // Assert
        result.TotalPages.Should().Be(totalPages);
        result.HasPrevious.Should().Be(hasPrev);
        result.HasNext.Should().Be(hasNext);
    }

    [Fact]
    public void HasNext_UltimaPagina_RetornaFalse()
    {
        // Arrange
        var page = new VetSync.Domain.Repositories.PageRequest { Page = 3, PageSize = 10 };

        // Act
        var result = PagedResult<string>.Create(new List<string>(), 25, page);

        // Assert
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }
}
