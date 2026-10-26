namespace VetSync.Application.DTOs;

public class JornadaPetDto
{
    public PetDto Pet { get; set; } = null!;
    public TutorDto Tutor { get; set; } = null!;
    public List<ConsultaDto> Consultas { get; set; } = new();
    public List<VacinacaoDto> Vacinacoes { get; set; } = new();
    public List<ExameDto> Exames { get; set; } = new();
}
