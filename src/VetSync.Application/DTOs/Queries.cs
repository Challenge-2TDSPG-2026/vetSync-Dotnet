using VetSync.Application.Common;

namespace VetSync.Application.DTOs;

public class TutorQuery : PageQuery
{
    /// <summary>Filtra por parte do nome.</summary>
    public string? Nome { get; set; }
    /// <summary>Filtra por e-mail exato.</summary>
    public string? Email { get; set; }
    public bool? Ativo { get; set; }
}

public class PetQuery : PageQuery
{
    public string? Nome { get; set; }
    public string? Especie { get; set; }
    public string? Raca { get; set; }
    public bool? Ativo { get; set; }
    public int? TutorId { get; set; }
}

public class ConsultaQuery : PageQuery
{
    public int? PetId { get; set; }
    public string? Status { get; set; }
    /// <summary>Data inicial (inclusive) da consulta.</summary>
    public DateTime? De { get; set; }
    /// <summary>Data final (inclusive) da consulta.</summary>
    public DateTime? Ate { get; set; }
}

public class VacinacaoQuery : PageQuery
{
    public int? PetId { get; set; }
    /// <summary>Retorna vacinações com próxima dose até esta data.</summary>
    public DateTime? ProximaDoseAte { get; set; }
}

public class ExameQuery : PageQuery
{
    public int? PetId { get; set; }
    /// <summary>Filtra por parte do tipo do exame.</summary>
    public string? Tipo { get; set; }
}
