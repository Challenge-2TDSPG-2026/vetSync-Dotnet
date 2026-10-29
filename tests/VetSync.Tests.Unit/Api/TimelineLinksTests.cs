using FluentAssertions;
using Microsoft.AspNetCore.Http;
using VetSync.Api.Hateoas;
using VetSync.Application.DTOs;
using Xunit;

namespace VetSync.Tests.Unit.Api;

public class TimelineLinksTests
{
    [Fact]
    public void WithLinks_EventoDaTimeline_IncluiSelfPetEJornada()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        var evento = new JornadaEventoDto { Id = "abc", PetId = 7 };

        // Act
        evento.WithLinks(ctx.Request);

        // Assert
        evento.Links.Should().Contain(l => l.Rel == "self" && l.Href == "/api/pets/7/timeline" && l.Method == "GET");
        evento.Links.Should().Contain(l => l.Rel == "pet" && l.Href == "/api/pets/7");
        evento.Links.Should().Contain(l => l.Rel == "jornada" && l.Href == "/api/pets/7/jornada");
    }

    [Fact]
    public void WithLinks_ComPathBase_PrefixaTodosOsLinks()
    {
        // Arrange
        var ctx = new DefaultHttpContext();
        ctx.Request.PathBase = "/vetsync";
        var evento = new JornadaEventoDto { PetId = 3 };

        // Act
        evento.WithLinks(ctx.Request);

        // Assert
        evento.Links.Should().OnlyContain(l => l.Href.StartsWith("/vetsync/api/pets/3"));
    }
}