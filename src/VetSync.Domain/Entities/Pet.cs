namespace VetSync.Domain.Entities;

/// <summary>Pet acompanhado pela clínica.</summary>
public class Pet
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raca { get; set; }
    public string? Sexo { get; set; }
    public DateTime? DataNascimento { get; set; }
    public decimal? Peso { get; set; }
    public string? Cor { get; set; }
    public bool Ativo { get; set; } = true;
    public int TutorId { get; set; }
    public Tutor Tutor { get; set; } = null!;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
    public ICollection<Vacinacao> Vacinacoes { get; set; } = new List<Vacinacao>();
    public ICollection<Exame> Exames { get; set; } = new List<Exame>();

    /// <summary>Idade completa em anos na data de referência, ou null se não há data de nascimento.</summary>
    public int? CalcularIdadeEmAnos(DateTime referencia)
    {
        if (DataNascimento is null || DataNascimento.Value.Date > referencia.Date)
            return null;

        var nascimento = DataNascimento.Value.Date;
        var idade = referencia.Year - nascimento.Year;
        if (nascimento > referencia.AddYears(-idade).Date)
            idade--;

        return idade;
    }

    public void Atualizar(string nome, string especie, string? raca, string? sexo,
        DateTime? dataNascimento, decimal? peso, string? cor, bool ativo)
    {
        Nome = nome;
        Especie = especie;
        Raca = raca;
        Sexo = sexo;
        DataNascimento = dataNascimento;
        Peso = peso;
        Cor = cor;
        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }
}
