namespace VetSync.Application.DTOs;

/// <summary>Evento da linha do tempo do pet (armazenado em MongoDB).</summary>
public class JornadaEventoDto : VetSync.Application.Common.LinkedResourceDto
{
    public string Id { get; set; } = string.Empty;
    public int PetId { get; set; }
    public string PetNome { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int? ReferenciaId { get; set; }
    public DateTime OcorridoEm { get; set; }
    public Dictionary<string, string> Detalhes { get; set; } = new();
}
