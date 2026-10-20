namespace VetSync.Domain.Repositories;

public class TutorFilter
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public bool? Ativo { get; set; }
}

public class PetFilter
{
    public string? Nome { get; set; }
    public string? Especie { get; set; }
    public string? Raca { get; set; }
    public bool? Ativo { get; set; }
    public int? TutorId { get; set; }
}

public class ConsultaFilter
{
    public int? PetId { get; set; }
    public string? Status { get; set; }
    public DateTime? De { get; set; }
    public DateTime? Ate { get; set; }
}

public class VacinacaoFilter
{
    public int? PetId { get; set; }
    public DateTime? ProximaDoseAte { get; set; }
}

public class ExameFilter
{
    public int? PetId { get; set; }
    public string? Tipo { get; set; }
}
