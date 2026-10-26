using VetSync.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace VetSync.Application.DTOs;

public class VacinacaoDto : LinkedResourceDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public string Vacina { get; set; } = string.Empty;
    public string? Fabricante { get; set; }
    public string? Lote { get; set; }
    public DateTime DataAplicacao { get; set; }
    public DateTime? ProximaDose { get; set; }
    public string? Veterinario { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class CreateVacinacaoDto
{
    /// <example>1</example>
    [Required]
    public int PetId { get; set; }

    /// <example>V10</example>
    [Required]
    [MaxLength(200)]
    public string Vacina { get; set; } = string.Empty;

    /// <example>Zoetis</example>
    [MaxLength(200)]
    public string? Fabricante { get; set; }

    /// <example>LOT2024A</example>
    [MaxLength(100)]
    public string? Lote { get; set; }

    /// <example>2025-05-19</example>
    [Required]
    public DateTime DataAplicacao { get; set; }

    /// <example>2026-05-19</example>
    public DateTime? ProximaDose { get; set; }

    /// <example>Dra. Ana Costa</example>
    [MaxLength(200)]
    public string? Veterinario { get; set; }
}

public class ExameDto : LinkedResourceDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public string TipoExame { get; set; } = string.Empty;
    public DateTime DataExame { get; set; }
    public string? Resultado { get; set; }
    public string? Laboratorio { get; set; }
    public string? Veterinario { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}

public class CreateExameDto
{
    /// <example>1</example>
    [Required]
    public int PetId { get; set; }

    /// <example>Hemograma completo</example>
    [Required]
    [MaxLength(200)]
    public string TipoExame { get; set; } = string.Empty;

    /// <example>2025-05-19</example>
    [Required]
    public DateTime DataExame { get; set; }

    /// <example>Todos os índices dentro do esperado</example>
    [MaxLength(2000)]
    public string? Resultado { get; set; }

    /// <example>LabVet SP</example>
    [MaxLength(200)]
    public string? Laboratorio { get; set; }

    /// <example>Dr. Carlos Lima</example>
    [MaxLength(200)]
    public string? Veterinario { get; set; }
}

public class UpdateExameDto
{
    /// <example>Hemograma completo</example>
    [Required]
    [MaxLength(200)]
    public string TipoExame { get; set; } = string.Empty;

    /// <example>2025-05-19</example>
    [Required]
    public DateTime DataExame { get; set; }

    /// <example>Todos os índices dentro do esperado</example>
    [MaxLength(2000)]
    public string? Resultado { get; set; }

    /// <example>LabVet SP</example>
    [MaxLength(200)]
    public string? Laboratorio { get; set; }

    /// <example>Dr. Carlos Lima</example>
    [MaxLength(200)]
    public string? Veterinario { get; set; }
}
