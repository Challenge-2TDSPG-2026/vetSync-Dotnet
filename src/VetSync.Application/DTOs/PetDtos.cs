using VetSync.Application.Common;
using System.ComponentModel.DataAnnotations;

namespace VetSync.Application.DTOs;

public class PetDto : LinkedResourceDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raca { get; set; }
    public string? Sexo { get; set; }
    public DateTime? DataNascimento { get; set; }
    public decimal? Peso { get; set; }
    public string? Cor { get; set; }
    public bool Ativo { get; set; }
    public int TutorId { get; set; }
    public string TutorNome { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}

public class CreatePetDto
{
    /// <example>Rex</example>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <example>Cachorro</example>
    [Required(ErrorMessage = "Espécie é obrigatória")]
    [MaxLength(50)]
    public string Especie { get; set; } = string.Empty;

    /// <example>Golden Retriever</example>
    [MaxLength(100)]
    public string? Raca { get; set; }

    /// <example>Macho</example>
    [MaxLength(10)]
    public string? Sexo { get; set; }

    /// <example>2020-05-10</example>
    public DateTime? DataNascimento { get; set; }

    /// <example>12.5</example>
    public decimal? Peso { get; set; }

    /// <example>Dourado</example>
    [MaxLength(50)]
    public string? Cor { get; set; }

    /// <example>1</example>
    [Required(ErrorMessage = "TutorId é obrigatório")]
    public int TutorId { get; set; }
}

public class UpdatePetDto
{
    /// <example>Rex</example>
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    /// <example>Cachorro</example>
    [Required]
    [MaxLength(50)]
    public string Especie { get; set; } = string.Empty;

    /// <example>Golden Retriever</example>
    [MaxLength(100)]
    public string? Raca { get; set; }

    /// <example>Macho</example>
    [MaxLength(10)]
    public string? Sexo { get; set; }

    /// <example>2020-05-10</example>
    public DateTime? DataNascimento { get; set; }

    /// <example>13.0</example>
    public decimal? Peso { get; set; }

    /// <example>Dourado</example>
    [MaxLength(50)]
    public string? Cor { get; set; }

    /// <example>true</example>
    public bool Ativo { get; set; }
}
