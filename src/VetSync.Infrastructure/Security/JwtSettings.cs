namespace VetSync.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "VetSync.API";
    public string Audience { get; set; } = "VetSync.Clients";

    /// <summary>Chave simétrica (mín. 32 caracteres). Informe via variável de ambiente Jwt__Key / user-secrets.</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
