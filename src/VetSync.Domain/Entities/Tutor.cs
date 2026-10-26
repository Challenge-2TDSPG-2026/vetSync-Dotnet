namespace VetSync.Domain.Entities;

/// <summary>Tutor (dono) de um ou mais pets.</summary>
public class Tutor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public string? Endereco { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public void Atualizar(string nome, string email, string? telefone, string? cpf, string? endereco, bool ativo)
    {
        Nome = nome;
        Email = email;
        Telefone = telefone;
        Cpf = cpf;
        Endereco = endereco;
        Ativo = ativo;
        AtualizadoEm = DateTime.UtcNow;
    }
}
