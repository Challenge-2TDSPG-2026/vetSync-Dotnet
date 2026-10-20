using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Documents;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class PetServiceTests
{
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<ITutorRepository> _tutores = new();
    private readonly Mock<IJornadaEventoRepository> _jornada = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly PetService _sut;

    public PetServiceTests()
    {
        _sut = new PetService(_pets.Object, _tutores.Object, _jornada.Object, _uow.Object, NullLogger<PetService>.Instance);
    }

    private static Pet PetComTutor(int id = 1) => new()
    {
        Id = id, Nome = "Rex", Especie = "Cão", TutorId = 7, Tutor = new Tutor { Id = 7, Nome = "Maria", Email = "m@m.com" }
    };

    [Fact]
    public async Task CreateAsync_TutorInexistente_LancaNotFoundException()
    {
        // Arrange
        _tutores.Setup(t => t.ExistsAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateAsync(new CreatePetDto { Nome = "Rex", Especie = "Cão", TutorId = 7 });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _pets.Verify(p => p.AddAsync(It.IsAny<Pet>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TutorExistente_PersisteERecarregaComNomeDoTutor()
    {
        // Arrange
        _tutores.Setup(t => t.ExistsAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _pets.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(PetComTutor(0));

        // Act
        var dto = await _sut.CreateAsync(new CreatePetDto { Nome = "Rex", Especie = "Cão", TutorId = 7 });

        // Assert
        dto.TutorNome.Should().Be("Maria");
        _pets.Verify(p => p.AddAsync(It.Is<Pet>(x => x.Nome == "Rex" && x.TutorId == 7), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_TutorIdInexistente_LancaNotFoundException()
    {
        // Arrange
        _tutores.Setup(t => t.ExistsAsync(55, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => _sut.GetPagedAsync(new PetQuery { TutorId = 55 });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPagedAsync_OrdenacaoPadrao_OrdenaPorNomeAscendente()
    {
        // Arrange
        PageRequest? recebida = null;
        _pets.Setup(p => p.SearchAsync(It.IsAny<PetFilter>(), It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PetFilter, PageRequest, CancellationToken>((_, page, _) => recebida = page)
            .ReturnsAsync(new PagedList<Pet>(new List<Pet> { PetComTutor() }, 1));

        // Act
        var result = await _sut.GetPagedAsync(new PetQuery { Especie = "Cão" });

        // Assert
        recebida!.SortBy.Should().Be("nome");
        recebida.Descending.Should().BeFalse();
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetJornadaAsync_PetComHistorico_RetornaItensOrdenadosDoMaisRecente()
    {
        // Arrange
        var pet = PetComTutor();
        pet.Consultas.Add(new Consulta { Id = 1, DataConsulta = new DateTime(2026, 1, 1), Veterinario = "A" });
        pet.Consultas.Add(new Consulta { Id = 2, DataConsulta = new DateTime(2026, 6, 1), Veterinario = "B" });
        _pets.Setup(p => p.GetWithJornadaAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pet);

        // Act
        var jornada = await _sut.GetJornadaAsync(1);

        // Assert
        jornada.Tutor.Nome.Should().Be("Maria");
        jornada.Consultas.Select(c => c.Id).Should().ContainInOrder(2, 1);
    }

    [Fact]
    public async Task GetJornadaAsync_PetInexistente_LancaNotFoundException()
    {
        // Arrange
        _pets.Setup(p => p.GetWithJornadaAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var act = () => _sut.GetJornadaAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTimelineAsync_LimiteAcimaDoMaximo_LimitaAQuantidadePedidaAoMongo()
    {
        // Arrange
        _pets.Setup(p => p.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _jornada.Setup(j => j.GetByPetAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JornadaEvento> { new() { PetId = 1, TipoEvento = JornadaEvento.VacinaAplicada } });

        // Act
        var eventos = await _sut.GetTimelineAsync(1, 100000);

        // Assert
        eventos.Should().ContainSingle(e => e.TipoEvento == "VacinaAplicada");
        _jornada.Verify(j => j.GetByPetAsync(1, 200, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimelineAsync_PetInexistente_LancaNotFoundExceptionSemConsultarMongo()
    {
        // Arrange
        _pets.Setup(p => p.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => _sut.GetTimelineAsync(1, 10);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _jornada.Verify(j => j.GetByPetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_PetExistente_AtualizaCamposESalva()
    {
        // Arrange
        var pet = PetComTutor();
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pet);

        // Act
        await _sut.UpdateAsync(1, new UpdatePetDto { Nome = "Thor", Especie = "Cão", Ativo = false });

        // Assert
        pet.Nome.Should().Be("Thor");
        pet.Ativo.Should().BeFalse();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_PetInexistente_LancaNotFoundException()
    {
        // Arrange
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var act = () => _sut.DeleteAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
