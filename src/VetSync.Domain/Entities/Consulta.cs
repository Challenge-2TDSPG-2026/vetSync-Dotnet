using VetSync.Domain.Exceptions;

namespace VetSync.Domain.Entities;

/// <summary>Consulta veterinária agendada/realizada para um pet.</summary>
public class Consulta
{
    public const string StatusAgendada = "Agendada";
    public const string StatusEmAndamento = "Em andamento";
    public const string StatusRealizada = "Realizada";
    public const string StatusCancelada = "Cancelada";

    public static readonly string[] StatusValidos =
        { StatusAgendada, StatusRealizada, StatusCancelada, StatusEmAndamento };

    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;
    public string Veterinario { get; set; } = string.Empty;
    public DateTime DataConsulta { get; set; }
    public string? Motivo { get; set; }
    public string? Diagnostico { get; set; }
    public string? Prescricao { get; set; }
    public string Status { get; set; } = StatusAgendada;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public static bool StatusEhValido(string? status)
        => status is not null && StatusValidos.Contains(status);

    /// <summary>Atualiza a consulta validando o status informado.</summary>
    public void Atualizar(string veterinario, DateTime dataConsulta, string? motivo,
        string? diagnostico, string? prescricao, string status)
    {
        if (!StatusEhValido(status))
            throw new BusinessRuleException($"Status inválido. Aceitos: {string.Join(", ", StatusValidos)}");

        Veterinario = veterinario;
        DataConsulta = dataConsulta;
        Motivo = motivo;
        Diagnostico = diagnostico;
        Prescricao = prescricao;
        Status = status;
        AtualizadoEm = DateTime.UtcNow;
    }
}
