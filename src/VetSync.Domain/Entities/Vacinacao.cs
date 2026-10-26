using VetSync.Domain.Exceptions;

namespace VetSync.Domain.Entities;

/// <summary>Registro de vacina aplicada em um pet.</summary>
public class Vacinacao
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public string Vacina { get; set; } = string.Empty;
    public string? Fabricante { get; set; }
    public string? Lote { get; set; }
    public DateTime DataAplicacao { get; set; }
    public DateTime? ProximaDose { get; set; }
    public string? Veterinario { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    /// <summary>Garante a consistência entre a data de aplicação e a próxima dose.</summary>
    public void Validar()
    {
        if (ProximaDose.HasValue && ProximaDose.Value.Date < DataAplicacao.Date)
            throw new BusinessRuleException("A próxima dose não pode ser anterior à data de aplicação.");
    }

    /// <summary>Indica se a próxima dose está vencida na data de referência.</summary>
    public bool ProximaDoseVencida(DateTime referencia)
        => ProximaDose.HasValue && ProximaDose.Value.Date < referencia.Date;
}
