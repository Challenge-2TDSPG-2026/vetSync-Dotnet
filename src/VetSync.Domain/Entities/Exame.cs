namespace VetSync.Domain.Entities;

/// <summary>Exame realizado em um pet.</summary>
public class Exame
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public string TipoExame { get; set; } = string.Empty;
    public DateTime DataExame { get; set; }
    public string? Resultado { get; set; }
    public string? Laboratorio { get; set; }
    public string? Veterinario { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public bool PossuiResultado => !string.IsNullOrWhiteSpace(Resultado);

    public void Atualizar(string tipoExame, DateTime dataExame, string? resultado,
        string? laboratorio, string? veterinario)
    {
        TipoExame = tipoExame;
        DataExame = dataExame;
        Resultado = resultado;
        Laboratorio = laboratorio;
        Veterinario = veterinario;
        AtualizadoEm = DateTime.UtcNow;
    }
}
