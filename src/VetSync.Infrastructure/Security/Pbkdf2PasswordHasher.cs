using System.Security.Cryptography;
using VetSync.Application.Abstractions;

namespace VetSync.Infrastructure.Security;

/// <summary>Hash de senhas com PBKDF2-HMAC-SHA256 e salt aleatório. Formato: v1.iterações.salt.hash (Base64).</summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string senha, string hash)
    {
        var partes = hash.Split('.');
        if (partes.Length != 4 || partes[0] != "v1" || !int.TryParse(partes[1], out var iteracoes))
            return false;

        try
        {
            var salt = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
