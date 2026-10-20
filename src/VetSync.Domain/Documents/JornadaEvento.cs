namespace VetSync.Domain.Documents;

/// <summary>
/// Documento NoSQL (MongoDB) que registra cada evento da jornada contínua de cuidado do pet.
/// É um read model denormalizado: guarda tudo que é necessário para montar a linha do tempo
/// sem precisar de JOINs no banco relacional.
/// </summary>
public class JornadaEvento
{
    public const string ConsultaAgendada = "ConsultaAgendada";
    public const string ConsultaAtualizada = "ConsultaAtualizada";
    public const string VacinaAplicada = "VacinaAplicada";
    public const string ExameRegistrado = "ExameRegistrado";
    public const string ExameAtualizado = "ExameAtualizado";

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int? ReferenciaId { get; set; }
    public DateTime OcorridoEm { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Detalhes { get; set; } = new();
}
