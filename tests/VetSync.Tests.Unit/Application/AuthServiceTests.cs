using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VetSync.Application.Abstractions;
using VetSync.Application.DTOs;
using VetSync.Application.Services;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;
using Xunit;

namespace VetSync.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarios = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenGenerator> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("HASH");
        _tokens.Setup(t => t.Generate(It.IsAny<Usuario>())).Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));
        _sut = new AuthService(_usuarios.Object, _uow.Object, _hasher.Object, _tokens.Object, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_DadosValidos_CriaUsuarioComPerfilUsuarioEDevolveToken()
    {
        // Arrange
        _usuarios.Setup(u => u.EmailExistsAsync("maria@vet.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var resposta = await _sut.RegisterAsync(new RegisterDto { Nome = "Maria", Email = "MARIA@vet.com", Senha = "Senha123" });

        // Assert
        resposta.Token.Should().Be("jwt-token");
        resposta.Role.Should().Be(Roles.Usuario);
        resposta.Email.Should().Be("maria@vet.com");
        _usuarios.Verify(u => u.AddAsync(It.Is<Usuario>(x => x.SenhaHash == "HASH" && x.Role == Roles.Usuario), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("curta1")]
    [InlineData("somenteletras")]
    [InlineData("12345678")]
    public async Task RegisterAsync_SenhaFraca_LancaBusinessRuleException(string senha)
    {
        // Arrange / Act
        var act = () => _sut.RegisterAsync(new RegisterDto { Nome = "Maria", Email = "m@vet.com", Senha = senha });

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
        _usuarios.Verify(u => u.AddAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_EmailJaCadastrado_LancaConflictException()
    {
        // Arrange
        _usuarios.Setup(u => u.EmailExistsAsync("maria@vet.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var act = () => _sut.RegisterAsync(new RegisterDto { Nome = "Maria", Email = "maria@vet.com", Senha = "Senha123" });

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task LoginAsync_CredenciaisCorretas_DevolveToken()
    {
        // Arrange
        var usuario = Usuario.Criar("Admin", "admin@vet.com", "HASH", Roles.Admin);
        _usuarios.Setup(u => u.GetByEmailAsync("admin@vet.com", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _hasher.Setup(h => h.Verify("Admin@123", "HASH")).Returns(true);

        // Act
        var resposta = await _sut.LoginAsync(new LoginDto { Email = "Admin@vet.com", Senha = "Admin@123" });

        // Assert
        resposta.Token.Should().Be("jwt-token");
        resposta.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_LancaAuthenticationFailedException()
    {
        // Arrange
        _usuarios.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Usuario?)null);

        // Act
        var act = () => _sut.LoginAsync(new LoginDto { Email = "x@vet.com", Senha = "qualquer1" });

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task LoginAsync_SenhaIncorreta_LancaAuthenticationFailedException()
    {
        // Arrange
        var usuario = Usuario.Criar("Maria", "m@vet.com", "HASH", Roles.Usuario);
        _usuarios.Setup(u => u.GetByEmailAsync("m@vet.com", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), "HASH")).Returns(false);

        // Act
        var act = () => _sut.LoginAsync(new LoginDto { Email = "m@vet.com", Senha = "errada123" });

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task LoginAsync_UsuarioInativo_LancaAuthenticationFailedException()
    {
        // Arrange
        var usuario = Usuario.Criar("Maria", "m@vet.com", "HASH", Roles.Usuario);
        usuario.Ativo = false;
        _usuarios.Setup(u => u.GetByEmailAsync("m@vet.com", It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act
        var act = () => _sut.LoginAsync(new LoginDto { Email = "m@vet.com", Senha = "Senha123" });

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
