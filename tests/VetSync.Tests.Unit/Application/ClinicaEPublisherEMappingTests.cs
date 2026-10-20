using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetSync.Application.Abstractions;
using VetSync.Application.Mappings;
using VetSync.Application.Services;
using VetSync.Domain.Documents;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class ClinicaServiceTests
{
    private readonly Mock<IVeterinaryClinicSearchService> _search = new();
    private readonly Mock<IGeoLocationService> _geo = new();
    private readonly ClinicaService _sut;

    public ClinicaServiceTests()
    {
        _sut = new ClinicaService(_search.Object, _geo.Object, NullLogger<ClinicaService>.Instance);
    }

    [Fact]
    public async Task BuscarProximasAsync_CoordenadasInvalidas_LancaBusinessRuleException()
    {
        // Arrange
        _geo.Setup(g => g.CoordenadasValidas(200, 0)).Returns(false);

        // Act
        var act = () => _sut.BuscarProximasAsync(200, 0, 10);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*Latitude*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(50.1)]
    public async Task BuscarProximasAsync_RaioForaDoLimite_LancaBusinessRuleException(double raio)
    {
        // Arrange
        _geo.Setup(g => g.CoordenadasValidas(It.IsAny<double>(), It.IsAny<double>())).Returns(true);

        // Act
        var act = () => _sut.BuscarProximasAsync(-23.5, -46.6, raio);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task BuscarProximasAsync_ParametrosValidos_ConverteResultadoParaDto()
    {
        // Arrange
        _geo.Setup(g => g.CoordenadasValidas(It.IsAny<double>(), It.IsAny<double>())).Returns(true);
        _search.Setup(s => s.BuscarProximasAsync(-23.5, -46.6, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ClinicaVeterinariaEncontrada>
            {
                new() { Id = "node/1", Nome = "Vet Centro", DistanciaKm = 1.2 }
            });

        // Act
        var resultado = await _sut.BuscarProximasAsync(-23.5, -46.6, 10);

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Nome.Should().Be("Vet Centro");
        resultado[0].LinkMapa.Should().Be("https://www.openstreetmap.org/node/1");
    }
}

public class JornadaEventPublisherTests
{
    [Fact]
    public async Task PublicarAsync_RepositorioDisponivel_GravaOEvento()
    {
        // Arrange
        var repo = new Mock<IJornadaEventoRepository>();
        var sut = new JornadaEventPublisher(repo.Object, NullLogger<JornadaEventPublisher>.Instance);
        var evento = new JornadaEvento { PetId = 1, TipoEvento = JornadaEvento.VacinaAplicada };

        // Act
        await sut.PublicarAsync(evento);

        // Assert
        repo.Verify(r => r.AddAsync(evento, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublicarAsync_MongoFalha_NaoPropagaExcecao()
    {
        // Arrange
        var repo = new Mock<IJornadaEventoRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<JornadaEvento>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Mongo fora do ar"));
        var sut = new JornadaEventPublisher(repo.Object, NullLogger<JornadaEventPublisher>.Instance);

        // Act
        var act = () => sut.PublicarAsync(new JornadaEvento { PetId = 1 });

        // Assert
        await act.Should().NotThrowAsync();
    }
}

public class MappingExtensionsTests
{
    [Fact]
    public void ToDto_Pet_CopiaCamposENomeDoTutor()
    {
        // Arrange
        var pet = new Pet { Id = 4, Nome = "Rex", Especie = "Cão", TutorId = 2, Tutor = new Tutor { Nome = "Maria" } };

        // Act
        var dto = pet.ToDto();

        // Assert
        dto.Id.Should().Be(4);
        dto.TutorNome.Should().Be("Maria");
        dto.Links.Should().BeEmpty();
    }

    [Fact]
    public void ToDto_ConsultaSemPetCarregado_NaoQuebraEUsaTextoVazio()
    {
        // Arrange
        var consulta = new Consulta { Id = 1, Veterinario = "A" };

        // Act
        var dto = consulta.ToDto();

        // Assert
        dto.PetNome.Should().BeEmpty();
        dto.TutorNome.Should().BeEmpty();
    }

    [Fact]
    public void ToEntity_CreateTutor_AparaEspacosENormalizaCpfVazioParaNulo()
    {
        // Arrange
        var dto = new VetSync.Application.DTOs.CreateTutorDto { Nome = "  Ana ", Email = " ana@a.com ", Cpf = "   " };

        // Act
        var tutor = dto.ToEntity();

        // Assert
        tutor.Nome.Should().Be("Ana");
        tutor.Email.Should().Be("ana@a.com");
        tutor.Cpf.Should().BeNull();
    }
}
