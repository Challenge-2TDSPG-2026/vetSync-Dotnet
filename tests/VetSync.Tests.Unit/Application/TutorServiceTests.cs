using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class TutorServiceTests
{
    private readonly Mock<ITutorRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly TutorService _sut;

    public TutorServiceTests()
    {
        _sut = new TutorService(_repo.Object, _uow.Object, NullLogger<TutorService>.Instance);
    }

    private static CreateTutorDto NovoDto() => new()
    {
        Nome = "  Maria Souza ", Email = "maria@email.com", Cpf = "123.456.789-00"
    };

    [Fact]
    public async Task CreateAsync_DadosValidos_PersisteEDevolveDto()
    {
        // Arrange
        _repo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.CpfExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var dto = await _sut.CreateAsync(NovoDto());

        // Assert
        dto.Nome.Should().Be("Maria Souza");
        dto.Email.Should().Be("maria@email.com");
        _repo.Verify(r => r.AddAsync(It.Is<Tutor>(t => t.Email == "maria@email.com"), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmailJaCadastrado_LancaConflictExceptionESemSalvar()
    {
        // Arrange
        _repo.Setup(r => r.EmailExistsAsync("maria@email.com", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(NovoDto());

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*e-mail*");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CpfJaCadastrado_LancaConflictException()
    {
        // Arrange
        _repo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repo.Setup(r => r.CpfExistsAsync("123.456.789-00", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(NovoDto());

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*CPF*");
    }

    [Fact]
    public async Task GetByIdAsync_TutorInexistente_LancaNotFoundException()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Tutor?)null);

        // Act
        var act = () => _sut.GetByIdAsync(99);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPagedAsync_ComFiltros_RepassaFiltroEPaginacaoAoRepositorio()
    {
        // Arrange
        var tutor = new Tutor { Id = 1, Nome = "Ana", Email = "ana@a.com" };
        TutorFilter? filtroRecebido = null;
        PageRequest? paginaRecebida = null;
        _repo.Setup(r => r.SearchAsync(It.IsAny<TutorFilter>(), It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .Callback<TutorFilter, PageRequest, CancellationToken>((f, p, _) => { filtroRecebido = f; paginaRecebida = p; })
            .ReturnsAsync(new PagedList<Tutor>(new List<Tutor> { tutor }, 11));

        // Act
        var result = await _sut.GetPagedAsync(new TutorQuery { Nome = "An", Page = 2, PageSize = 5, SortBy = "email", SortDir = "desc" });

        // Assert
        filtroRecebido!.Nome.Should().Be("An");
        paginaRecebida!.SortBy.Should().Be("email");
        paginaRecebida.Descending.Should().BeTrue();
        result.TotalCount.Should().Be(11);
        result.TotalPages.Should().Be(3);
        result.Items.Should().ContainSingle(t => t.Id == 1);
    }

    [Fact]
    public async Task GetPagedAsync_CampoDeOrdenacaoInvalido_LancaBusinessRuleException()
    {
        // Arrange
        var query = new TutorQuery { SortBy = "cpf" };

        // Act
        var act = () => _sut.GetPagedAsync(query);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task UpdateAsync_EmailPertenceAOutroTutor_LancaConflictException()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Tutor { Id = 1 });
        _repo.Setup(r => r.EmailExistsAsync("outro@email.com", 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _sut.UpdateAsync(1, new UpdateTutorDto { Nome = "A", Email = "outro@email.com", Ativo = true });

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DadosValidos_AtualizaEntidadeESalva()
    {
        // Arrange
        var tutor = new Tutor { Id = 1, Nome = "Velho", Email = "velho@a.com" };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);

        // Act
        var dto = await _sut.UpdateAsync(1, new UpdateTutorDto { Nome = "Novo", Email = "novo@a.com", Ativo = false });

        // Assert
        tutor.Nome.Should().Be("Novo");
        tutor.Ativo.Should().BeFalse();
        dto.AtualizadoEm.Should().NotBeNull();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TutorComPets_LancaConflictException()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Tutor { Id = 1 });
        _repo.Setup(r => r.HasPetsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _sut.DeleteAsync(1);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _repo.Verify(r => r.Remove(It.IsAny<Tutor>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_TutorSemPets_RemoveESalva()
    {
        // Arrange
        var tutor = new Tutor { Id = 1 };
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(tutor);
        _repo.Setup(r => r.HasPetsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        _repo.Verify(r => r.Remove(tutor), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
