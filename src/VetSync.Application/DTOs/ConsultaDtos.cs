using VetSync.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace VetSync.Application.DTOs;

public class ConsultaDto : LinkedResourceDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public string TutorNome { get; set; } = string.Empty;
    public string Veterinario { get; set; } = string.Empty;
    public DateTime DataConsulta { get; set; }
    public string? Motivo { get; set; }
    public string? Diagnostico { get; set; }
    public string? Prescricao { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}

public class CreateConsultaDto
{
    /// <example>1</example>
    [Required(ErrorMessage = "PetId é obrigatório")]
    public int PetId { get; set; }

    /// <example>Dr. João Silva</example>
    [Required(ErrorMessage = "Veterinário é obrigatório")]
    [MaxLength(200)]
    public string Veterinario { get; set; } = string.Empty;

    /// <example>2025-06-15T10:00:00</example>
    [Required(ErrorMessage = "Data da consulta é obrigatória")]
    public DateTime DataConsulta { get; set; }

    /// <example>Check-up anual e queda de pelo</example>
    [MaxLength(2000)]
    public string? Motivo { get; set; }
}

public class UpdateConsultaDto
{
    /// <example>Dr. João Silva</example>
    [Required]
    [MaxLength(200)]
    public string Veterinario { get; set; } = string.Empty;

    /// <example>2025-06-15T10:00:00</example>
    [Required]
    public DateTime DataConsulta { get; set; }

    /// <example>Check-up anual</example>
    [MaxLength(2000)]
    public string? Motivo { get; set; }

    /// <example>Animal saudável, leve dermatite</example>
    [MaxLength(2000)]
    public string? Diagnostico { get; set; }

    /// <example>Shampoo antifúngico 2x por semana por 30 dias</example>
    [MaxLength(2000)]
    public string? Prescricao { get; set; }

    /// <example>Realizada</example>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
