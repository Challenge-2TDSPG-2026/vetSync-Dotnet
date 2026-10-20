using System.ComponentModel.DataAnnotations;

namespace VetSync.Application.DTOs;

public class RegisterDto
{
    /// <example>Maria Souza</example>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <example>maria@vetsync.com</example>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <example>Senha@123</example>
    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres")]
    [MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}

public class LoginDto
{
    /// <example>admin@vetsync.com</example>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <example>Admin@123</example>
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string TipoToken { get; set; } = "Bearer";
    public DateTime ExpiraEm { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
