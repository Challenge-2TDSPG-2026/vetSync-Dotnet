using VetSync.Domain.Exceptions;

namespace VetSync.Domain.Entities;

/// <summary>Usuário da API (autenticação JWT).</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.Usuario;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public static Usuario Criar(string nome, string email, string senhaHash, string role)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessRuleException("Nome do usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleException("E-mail do usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new BusinessRuleException("Hash de senha é obrigatório.");
        if (!Roles.Todos.Contains(role))
            throw new BusinessRuleException($"Perfil inválido. Aceitos: {string.Join(", ", Roles.Todos)}.");

        return new Usuario
        {
            Nome = nome.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            SenhaHash = senhaHash,
            Role = role
        };
    }
}
