using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetSync.Application.Abstractions;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Documents;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class ConsultaServiceTests
{
    private readonly Mock<IConsultaRepository> _consultas = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IJornadaEventPublisher> _jornada = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ConsultaService _sut;

    public ConsultaServiceTests()
    {
        _sut = new ConsultaService(_consultas.Object, _pets.Object, _jornada.Object, _uow.Object,
            NullLogger<ConsultaService>.Instance);
    }

    private static Pet NovoPet() => new() { Id = 3, Nome = "Rex", Tutor = new Tutor { Nome = "Maria" } };

    [Fact]
    public async Task CreateAsync_PetInexistente_LancaNotFoundExceptionSemSalvar()
    {
        // Arrange
        _pets.Setup(p => p.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var act = () => _sut.CreateAsync(new CreateConsultaDto { PetId = 3, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PetExistente_SalvaEPublicaEventoConsultaAgendada()
    {
        // Arrange
        var pet = NovoPet();
        _pets.Setup(p => p.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(pet);
        _consultas.Setup(c => c.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Consulta { Id = 10, PetId = 3, Pet = pet, Veterinario = "Dra. Ana" });

        // Act
        var dto = await _sut.CreateAsync(new CreateConsultaDto { PetId = 3, Veterinario = "Dra. Ana", DataConsulta = DateTime.UtcNow });

        // Assert
        dto.PetNome.Should().Be("Rex");
        _consultas.Verify(c => c.AddAsync(It.Is<Consulta>(x => x.Status == Consulta.StatusAgendada), It.IsAny<CancellationToken>()), Times.Once);
        _jornada.Verify(j => j.PublicarAsync(
            It.Is<JornadaEvento>(e => e.PetId == 3 && e.TipoEvento == JornadaEvento.ConsultaAgendada),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_StatusInvalido_LancaBusinessRuleExceptionSemSalvarNemPublicar()
    {
        // Arrange
        _consultas.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Consulta { Id = 1, PetId = 3, Veterinario = "A" });

        // Act
        var act = () => _sut.UpdateAsync(1, new UpdateConsultaDto { Veterinario = "B", DataConsulta = DateTime.UtcNow, Status = "XYZ" });

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _jornada.Verify(j => j.PublicarAsync(It.IsAny<JornadaEvento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_StatusRealizada_SalvaEPublicaEventoConsultaAtualizada()
    {
        // Arrange
        var consulta = new Consulta { Id = 1, PetId = 3, Pet = NovoPet(), Veterinario = "A" };
        _consultas.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        // Act
        var dto = await _sut.UpdateAsync(1, new UpdateConsultaDto
        {
            Veterinario = "B", DataConsulta = DateTime.UtcNow, Status = Consulta.StatusRealizada, Diagnostico = "Saudável"
        });

        // Assert
        dto.Status.Should().Be("Realizada");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _jornada.Verify(j => j.PublicarAsync(
            It.Is<JornadaEvento>(e => e.TipoEvento == JornadaEvento.ConsultaAtualizada), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_StatusInvalido_LancaBusinessRuleException()
    {
        // Arrange / Act
        var act = () => _sut.GetPagedAsync(new ConsultaQuery { Status = "Nada" });

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task GetPagedAsync_DataFinalSemHora_IncluiODiaInteiro()
    {
        // Arrange
        ConsultaFilter? filtro = null;
        _consultas.Setup(c => c.SearchAsync(It.IsAny<ConsultaFilter>(), It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ConsultaFilter, PageRequest, CancellationToken>((f, _, _) => filtro = f)
            .ReturnsAsync(new PagedList<Consulta>(new List<Consulta>(), 0));

        // Act
        await _sut.GetPagedAsync(new ConsultaQuery { Ate = new DateTime(2026, 10, 19) });

        // Assert
        filtro!.Ate.Should().Be(new DateTime(2026, 10, 19, 23, 59, 59).AddTicks(9999999));
    }

    [Fact]
    public async Task GetPagedAsync_OrdenacaoPadrao_UsaDataDaConsultaDescendente()
    {
        // Arrange
        PageRequest? page = null;
        _consultas.Setup(c => c.SearchAsync(It.IsAny<ConsultaFilter>(), It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ConsultaFilter, PageRequest, CancellationToken>((_, p, _) => page = p)
            .ReturnsAsync(new PagedList<Consulta>(new List<Consulta>(), 0));

        // Act
        await _sut.GetPagedAsync(new ConsultaQuery());

        // Assert
        page!.SortBy.Should().Be("dataConsulta");
        page.Descending.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ConsultaExistente_RemoveESalva()
    {
        // Arrange
        var consulta = new Consulta { Id = 1 };
        _consultas.Setup(c => c.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _consultas.Verify(c => c.Remove(consulta), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
