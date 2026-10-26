using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VetSync.Application.Abstractions;
using VetSync.Domain.Entities;

namespace VetSync.Infrastructure.Security;

public class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> options) => _settings = options.Value;

    public TokenResult Generate(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("name", usuario.Nome),
            new Claim("role", usuario.Role)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: credenciais);

        return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
