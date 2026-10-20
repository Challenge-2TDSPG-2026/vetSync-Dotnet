using VetSync.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace VetSync.Application.DTOs;

public class TutorDto : LinkedResourceDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public string? Endereco { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}

public class CreateTutorDto
{
    /// <example>Maria Souza</example>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <example>maria@email.com</example>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <example>11987654321</example>
    [MaxLength(20)]
    public string? Telefone { get; set; }

    /// <example>123.456.789-00</example>
    [MaxLength(14)]
    public string? Cpf { get; set; }

    /// <example>Rua das Flores, 100, São Paulo - SP</example>
    [MaxLength(500)]
    public string? Endereco { get; set; }
}

public class UpdateTutorDto
{
    /// <example>Maria Souza</example>
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <example>maria@email.com</example>
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <example>11987654321</example>
    [MaxLength(20)]
    public string? Telefone { get; set; }

    /// <example>123.456.789-00</example>
    [MaxLength(14)]
    public string? Cpf { get; set; }

    /// <example>Rua das Flores, 100, São Paulo - SP</example>
    [MaxLength(500)]
    public string? Endereco { get; set; }

    /// <example>true</example>
    public bool Ativo { get; set; }
}
