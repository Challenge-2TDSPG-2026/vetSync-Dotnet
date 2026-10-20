using Microsoft.Extensions.Logging;
using VetSync.Application.Abstractions;
using VetSync.Application.DTOs;
using VetSync.Domain.Entities;
using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokens;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUsuarioRepository usuarios, IUnitOfWork unitOfWork, IPasswordHasher hasher,
        ITokenGenerator tokens, ILogger<AuthService> logger)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        ValidarForcaDaSenha(dto.Senha);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _usuarios.EmailExistsAsync(email, ct))
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");

        // Auto-cadastro público sempre cria perfil "Usuario"; Admin só via seed/operação interna.
        var usuario = Usuario.Criar(dto.Nome, email, _hasher.Hash(dto.Senha), Roles.Usuario);

        await _usuarios.AddAsync(usuario, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Usuário {UsuarioId} registrado com perfil {Role}.", usuario.Id, usuario.Role);
        return CriarResposta(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarios.GetByEmailAsync(email, ct);

        if (usuario is null || !usuario.Ativo || !_hasher.Verify(dto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Falha de autenticação para o e-mail informado.");
            throw new AuthenticationFailedException("E-mail ou senha inválidos.");
        }

        _logger.LogInformation("Usuário {UsuarioId} autenticado.", usuario.Id);
        return CriarResposta(usuario);
    }

    private AuthResponseDto CriarResposta(Usuario usuario)
    {
        var token = _tokens.Generate(usuario);
        return new AuthResponseDto
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role
        };
    }

    internal static void ValidarForcaDaSenha(string senha)
    {
        if (string.IsNullOrEmpty(senha) || senha.Length < 8 ||
            !senha.Any(char.IsLetter) || !senha.Any(char.IsDigit))
            throw new BusinessRuleException("A senha deve ter no mínimo 8 caracteres, com letras e números.");
    }
}
