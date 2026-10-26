using VetSync.Domain.Entities;

namespace VetSync.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verify(string senha, string hash);
}

public record TokenResult(string Token, DateTime ExpiraEm);

public interface ITokenGenerator
{
    TokenResult Generate(Usuario usuario);
}
