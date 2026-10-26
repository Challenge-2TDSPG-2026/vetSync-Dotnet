using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetSync.Application.DTOs;
using VetSync.Application.Services;

namespace VetSync.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Cria uma conta (perfil Usuario) e já devolve o token JWT</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var resposta = await _authService.RegisterAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, resposta);
    }

    /// <summary>Autentica com e-mail e senha e devolve o token JWT (Bearer)</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Ok(await _authService.LoginAsync(dto, ct));
}
