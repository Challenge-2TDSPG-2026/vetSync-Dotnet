using VetSync.Application.DTOs;
using VetSync.Domain.Documents;
using VetSync.Domain.Entities;

namespace VetSync.Application.Mappings;

/// <summary>Conversões manuais Entidade &lt;-&gt; DTO (sem reflexão, explícitas e testáveis).</summary>
public static class MappingExtensions
{
    // ---------- Tutor ----------
    public static TutorDto ToDto(this Tutor t) => new()
    {
        Id = t.Id, Nome = t.Nome, Email = t.Email, Telefone = t.Telefone, Cpf = t.Cpf,
        Endereco = t.Endereco, Ativo = t.Ativo, CriadoEm = t.CriadoEm, AtualizadoEm = t.AtualizadoEm
    };

    public static Tutor ToEntity(this CreateTutorDto d) => new()
    {
        Nome = d.Nome.Trim(),
        Email = d.Email.Trim(),
        Telefone = d.Telefone,
        Cpf = string.IsNullOrWhiteSpace(d.Cpf) ? null : d.Cpf.Trim(),
        Endereco = d.Endereco
    };

    // ---------- Pet ----------
    public static PetDto ToDto(this Pet p) => new()
    {
        Id = p.Id, Nome = p.Nome, Especie = p.Especie, Raca = p.Raca, Sexo = p.Sexo,
        DataNascimento = p.DataNascimento, Peso = p.Peso, Cor = p.Cor, Ativo = p.Ativo,
        TutorId = p.TutorId, TutorNome = p.Tutor?.Nome ?? string.Empty,
        CriadoEm = p.CriadoEm, AtualizadoEm = p.AtualizadoEm
    };

    public static Pet ToEntity(this CreatePetDto d) => new()
    {
        Nome = d.Nome.Trim(), Especie = d.Especie.Trim(), Raca = d.Raca, Sexo = d.Sexo,
        DataNascimento = d.DataNascimento, Peso = d.Peso, Cor = d.Cor, TutorId = d.TutorId
    };

    // ---------- Consulta ----------
    public static ConsultaDto ToDto(this Consulta c) => new()
    {
        Id = c.Id, PetId = c.PetId, PetNome = c.Pet?.Nome ?? string.Empty,
        TutorNome = c.Pet?.Tutor?.Nome ?? string.Empty, Veterinario = c.Veterinario,
        DataConsulta = c.DataConsulta, Motivo = c.Motivo, Diagnostico = c.Diagnostico,
        Prescricao = c.Prescricao, Status = c.Status, CriadoEm = c.CriadoEm, AtualizadoEm = c.AtualizadoEm
    };

    public static Consulta ToEntity(this CreateConsultaDto d) => new()
    {
        PetId = d.PetId, Veterinario = d.Veterinario.Trim(), DataConsulta = d.DataConsulta, Motivo = d.Motivo
    };

    // ---------- Vacinação ----------
    public static VacinacaoDto ToDto(this Vacinacao v) => new()
    {
        Id = v.Id, PetId = v.PetId, PetNome = v.Pet?.Nome ?? string.Empty, Vacina = v.Vacina,
        Fabricante = v.Fabricante, Lote = v.Lote, DataAplicacao = v.DataAplicacao,
        ProximaDose = v.ProximaDose, Veterinario = v.Veterinario, CriadoEm = v.CriadoEm
    };

    public static Vacinacao ToEntity(this CreateVacinacaoDto d) => new()
    {
        PetId = d.PetId, Vacina = d.Vacina.Trim(), Fabricante = d.Fabricante, Lote = d.Lote,
        DataAplicacao = d.DataAplicacao, ProximaDose = d.ProximaDose, Veterinario = d.Veterinario
    };

    // ---------- Exame ----------
    public static ExameDto ToDto(this Exame e) => new()
    {
        Id = e.Id, PetId = e.PetId, PetNome = e.Pet?.Nome ?? string.Empty, TipoExame = e.TipoExame,
        DataExame = e.DataExame, Resultado = e.Resultado, Laboratorio = e.Laboratorio,
        Veterinario = e.Veterinario, CriadoEm = e.CriadoEm, AtualizadoEm = e.AtualizadoEm
    };

    public static Exame ToEntity(this CreateExameDto d) => new()
    {
        PetId = d.PetId, TipoExame = d.TipoExame.Trim(), DataExame = d.DataExame,
        Resultado = d.Resultado, Laboratorio = d.Laboratorio, Veterinario = d.Veterinario
    };

    // ---------- Jornada (MongoDB) ----------
    public static JornadaEventoDto ToDto(this JornadaEvento e) => new()
    {
        Id = e.Id, PetId = e.PetId, PetNome = e.PetNome, TipoEvento = e.TipoEvento,
        Descricao = e.Descricao, ReferenciaId = e.ReferenciaId, OcorridoEm = e.OcorridoEm,
        Detalhes = e.Detalhes
    };
}
