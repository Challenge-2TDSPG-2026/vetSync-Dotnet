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

public class VacinacaoServiceTests
{
    private readonly Mock<IVacinacaoRepository> _vacinacoes = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IJornadaEventPublisher> _jornada = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly VacinacaoService _sut;

    public VacinacaoServiceTests()
    {
        _sut = new VacinacaoService(_vacinacoes.Object, _pets.Object, _jornada.Object, _uow.Object,
            NullLogger<VacinacaoService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ProximaDoseAnteriorAAplicacao_LancaBusinessRuleExceptionSemSalvar()
    {
        // Arrange
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Pet { Id = 1, Nome = "Rex" });
        var dto = new CreateVacinacaoDto
        {
            PetId = 1, Vacina = "V10", DataAplicacao = new DateTime(2026, 5, 10), ProximaDose = new DateTime(2026, 4, 1)
        };

        // Act
        var act = () => _sut.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_SalvaEPublicaEventoVacinaAplicada()
    {
        // Arrange
        var pet = new Pet { Id = 1, Nome = "Rex" };
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pet);
        _vacinacoes.Setup(v => v.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Vacinacao { Id = 5, PetId = 1, Pet = pet, Vacina = "V10" });
        var dto = new CreateVacinacaoDto { PetId = 1, Vacina = "V10", DataAplicacao = new DateTime(2026, 5, 10) };

        // Act
        var criada = await _sut.CreateAsync(dto);

        // Assert
        criada.Vacina.Should().Be("V10");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _jornada.Verify(j => j.PublicarAsync(
            It.Is<JornadaEvento>(e => e.TipoEvento == JornadaEvento.VacinaAplicada && e.PetNome == "Rex"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PetInexistente_LancaNotFoundException()
    {
        // Arrange
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Pet?)null);

        // Act
        var act = () => _sut.CreateAsync(new CreateVacinacaoDto { PetId = 1, Vacina = "V10" });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPagedAsync_PetIdInexistente_LancaNotFoundException()
    {
        // Arrange
        _pets.Setup(p => p.ExistsAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var act = () => _sut.GetPagedAsync(new VacinacaoQuery { PetId = 9 });

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_VacinacaoInexistente_LancaNotFoundException()
    {
        // Arrange
        _vacinacoes.Setup(v => v.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Vacinacao?)null);

        // Act
        var act = () => _sut.DeleteAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

public class ExameServiceTests
{
    private readonly Mock<IExameRepository> _exames = new();
    private readonly Mock<IPetRepository> _pets = new();
    private readonly Mock<IJornadaEventPublisher> _jornada = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ExameService _sut;

    public ExameServiceTests()
    {
        _sut = new ExameService(_exames.Object, _pets.Object, _jornada.Object, _uow.Object,
            NullLogger<ExameService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_DadosValidos_SalvaEPublicaEventoExameRegistrado()
    {
        // Arrange
        var pet = new Pet { Id = 1, Nome = "Rex" };
        _pets.Setup(p => p.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pet);
        _exames.Setup(e => e.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Exame { Id = 2, PetId = 1, Pet = pet, TipoExame = "Hemograma" });

        // Act
        var dto = await _sut.CreateAsync(new CreateExameDto { PetId = 1, TipoExame = "Hemograma", DataExame = DateTime.UtcNow });

        // Assert
        dto.TipoExame.Should().Be("Hemograma");
        _jornada.Verify(j => j.PublicarAsync(
            It.Is<JornadaEvento>(e => e.TipoEvento == JornadaEvento.ExameRegistrado), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComResultado_AtualizaEPublicaDescricaoDeResultadoDisponivel()
    {
        // Arrange
        var exame = new Exame { Id = 2, PetId = 1, Pet = new Pet { Nome = "Rex" }, TipoExame = "Hemograma" };
        _exames.Setup(e => e.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(exame);
        JornadaEvento? publicado = null;
        _jornada.Setup(j => j.PublicarAsync(It.IsAny<JornadaEvento>(), It.IsAny<CancellationToken>()))
            .Callback<JornadaEvento, CancellationToken>((e, _) => publicado = e)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateAsync(2, new UpdateExameDto { TipoExame = "Hemograma", DataExame = DateTime.UtcNow, Resultado = "Normal" });

        // Assert
        exame.Resultado.Should().Be("Normal");
        publicado!.Descricao.Should().Contain("Resultado do exame");
    }

    [Fact]
    public async Task GetByIdAsync_ExameInexistente_LancaNotFoundException()
    {
        // Arrange
        _exames.Setup(e => e.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Exame?)null);

        // Act
        var act = () => _sut.GetByIdAsync(1);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPagedAsync_OrdenacaoInvalida_LancaBusinessRuleException()
    {
        // Arrange / Act
        var act = () => _sut.GetPagedAsync(new ExameQuery { SortBy = "resultado" });

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
