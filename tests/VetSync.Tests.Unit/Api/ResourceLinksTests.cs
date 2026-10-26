using FluentAssertions;
using Microsoft.AspNetCore.Http;
using VetSync.Api.Hateoas;
using VetSync.Application.Common;
using VetSync.Application.DTOs;
using Xunit;

namespace VetSync.Tests.Unit.Api;

public class ResourceLinksTests
{
    private static HttpRequest Requisicao(string path = "/api/pets", string query = "")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.Request.QueryString = new QueryString(query);
        return ctx.Request;
    }

    [Fact]
    public void WithLinks_Pet_IncluiAcoesRelacionadas()
    {
        // Arrange
        var pet = new PetDto { Id = 3, TutorId = 7 };

        // Act
        pet.WithLinks(Requisicao());

        // Assert
        pet.Links.Should().Contain(l => l.Rel == "self" && l.Href == "/api/pets/3" && l.Method == "GET");
        pet.Links.Should().Contain(l => l.Rel == "delete" && l.Method == "DELETE");
        pet.Links.Should().Contain(l => l.Rel == "tutor" && l.Href == "/api/tutores/7");
        pet.Links.Should().Contain(l => l.Rel == "timeline" && l.Href == "/api/pets/3/timeline");
        pet.Links.Should().Contain(l => l.Rel == "consultas" && l.Href == "/api/consultas?petId=3");
    }

    [Fact]
    public void WithLinks_Vacinacao_NaoOfereceAtualizacao()
    {
        // Arrange
        var vacina = new VacinacaoDto { Id = 1, PetId = 2 };

        // Act
        vacina.WithLinks(Requisicao("/api/vacinacoes"));

        // Assert
        vacina.Links.Select(l => l.Rel).Should().BeEquivalentTo(new[] { "self", "delete", "pet" });
    }

    [Fact]
    public void WithLinks_PaginaDoMeio_IncluiPrevENextPreservandoFiltros()
    {
        // Arrange
        var pagina = new PagedResult<PetDto>
        {
            Items = new List<PetDto> { new() { Id = 1, TutorId = 1 } },
            Page = 2, PageSize = 5, TotalCount = 14
        };

        // Act
        pagina.WithLinks(Requisicao("/api/pets", "?especie=gato&page=2&pageSize=5"));
        var rels = pagina.Links.Select(l => l.Rel).ToList();

        // Assert
        rels.Should().Contain(new[] { "self", "first", "prev", "next", "last" });
        pagina.Links.Single(l => l.Rel == "next").Href.Should().Contain("page=3").And.Contain("especie=gato");
        pagina.Links.Single(l => l.Rel == "last").Href.Should().Contain("page=3");
        pagina.Items[0].Links.Should().NotBeEmpty();
    }

    [Fact]
    public void WithLinks_PrimeiraPaginaUnica_NaoTemPrevNemNext()
    {
        // Arrange
        var pagina = new PagedResult<TutorDto> { Items = new List<TutorDto>(), Page = 1, PageSize = 10, TotalCount = 0 };

        // Act
        pagina.WithLinks(Requisicao("/api/tutores"));

        // Assert
        pagina.Links.Select(l => l.Rel).Should().NotContain(new[] { "prev", "next" });
        pagina.Links.Single(l => l.Rel == "last").Href.Should().Contain("page=1");
    }
}
