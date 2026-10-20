using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetSync.Domain.Entities;

namespace VetSync.Infrastructure.Persistence.Configurations;

// Mapeamento via Fluent API: mantém o Domain livre de atributos de persistência.

public class TutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> b)
    {
        b.ToTable("TUTORES");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Nome).HasColumnName("NOME").IsRequired().HasMaxLength(200);
        b.Property(x => x.Email).HasColumnName("EMAIL").IsRequired().HasMaxLength(200);
        b.Property(x => x.Telefone).HasColumnName("TELEFONE").HasMaxLength(20);
        b.Property(x => x.Cpf).HasColumnName("CPF").HasMaxLength(14);
        b.Property(x => x.Endereco).HasColumnName("ENDERECO").HasMaxLength(500);
        b.Property(x => x.Ativo).HasColumnName("ATIVO").HasDefaultValue(true);
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.Property(x => x.AtualizadoEm).HasColumnName("ATUALIZADO_EM");
        b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IDX_TUTORES_EMAIL");
        b.HasIndex(x => x.Cpf).IsUnique().HasDatabaseName("IDX_TUTORES_CPF");
    }
}

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> b)
    {
        b.ToTable("PETS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Nome).HasColumnName("NOME").IsRequired().HasMaxLength(100);
        b.Property(x => x.Especie).HasColumnName("ESPECIE").IsRequired().HasMaxLength(50);
        b.Property(x => x.Raca).HasColumnName("RACA").HasMaxLength(100);
        b.Property(x => x.Sexo).HasColumnName("SEXO").HasMaxLength(10);
        b.Property(x => x.DataNascimento).HasColumnName("DATA_NASCIMENTO");
        b.Property(x => x.Peso).HasColumnName("PESO").HasColumnType("NUMBER(8,2)");
        b.Property(x => x.Cor).HasColumnName("COR").HasMaxLength(50);
        b.Property(x => x.Ativo).HasColumnName("ATIVO").HasDefaultValue(true);
        b.Property(x => x.TutorId).HasColumnName("TUTOR_ID");
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.Property(x => x.AtualizadoEm).HasColumnName("ATUALIZADO_EM");
        b.HasIndex(x => x.Especie).HasDatabaseName("IDX_PETS_ESPECIE");
        b.HasIndex(x => x.TutorId).HasDatabaseName("IDX_PETS_TUTOR_ID");
        b.HasOne(x => x.Tutor)
            .WithMany(t => t.Pets)
            .HasForeignKey(x => x.TutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConsultaConfiguration : IEntityTypeConfiguration<Consulta>
{
    public void Configure(EntityTypeBuilder<Consulta> b)
    {
        b.ToTable("CONSULTAS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.PetId).HasColumnName("PET_ID");
        b.Property(x => x.Veterinario).HasColumnName("VETERINARIO").IsRequired().HasMaxLength(200);
        b.Property(x => x.DataConsulta).HasColumnName("DATA_CONSULTA");
        b.Property(x => x.Motivo).HasColumnName("MOTIVO").HasMaxLength(2000);
        b.Property(x => x.Diagnostico).HasColumnName("DIAGNOSTICO").HasMaxLength(2000);
        b.Property(x => x.Prescricao).HasColumnName("PRESCRICAO").HasMaxLength(2000);
        b.Property(x => x.Status).HasColumnName("STATUS").HasMaxLength(50).HasDefaultValue(Consulta.StatusAgendada);
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.Property(x => x.AtualizadoEm).HasColumnName("ATUALIZADO_EM");
        b.HasIndex(x => x.Status).HasDatabaseName("IDX_CONSULTAS_STATUS");
        b.HasIndex(x => x.PetId).HasDatabaseName("IDX_CONSULTAS_PET_ID");
        b.HasOne(x => x.Pet)
            .WithMany(p => p.Consultas)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class VacinacaoConfiguration : IEntityTypeConfiguration<Vacinacao>
{
    public void Configure(EntityTypeBuilder<Vacinacao> b)
    {
        b.ToTable("VACINACOES");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.PetId).HasColumnName("PET_ID");
        b.Property(x => x.Vacina).HasColumnName("VACINA").IsRequired().HasMaxLength(200);
        b.Property(x => x.Fabricante).HasColumnName("FABRICANTE").HasMaxLength(200);
        b.Property(x => x.Lote).HasColumnName("LOTE").HasMaxLength(100);
        b.Property(x => x.DataAplicacao).HasColumnName("DATA_APLICACAO");
        b.Property(x => x.ProximaDose).HasColumnName("PROXIMA_DOSE");
        b.Property(x => x.Veterinario).HasColumnName("VETERINARIO").HasMaxLength(200);
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.HasIndex(x => x.PetId).HasDatabaseName("IDX_VACINACOES_PET_ID");
        b.HasOne(x => x.Pet)
            .WithMany(p => p.Vacinacoes)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExameConfiguration : IEntityTypeConfiguration<Exame>
{
    public void Configure(EntityTypeBuilder<Exame> b)
    {
        b.ToTable("EXAMES");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.PetId).HasColumnName("PET_ID");
        b.Property(x => x.TipoExame).HasColumnName("TIPO_EXAME").IsRequired().HasMaxLength(200);
        b.Property(x => x.DataExame).HasColumnName("DATA_EXAME");
        b.Property(x => x.Resultado).HasColumnName("RESULTADO").HasMaxLength(2000);
        b.Property(x => x.Laboratorio).HasColumnName("LABORATORIO").HasMaxLength(200);
        b.Property(x => x.Veterinario).HasColumnName("VETERINARIO").HasMaxLength(200);
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.Property(x => x.AtualizadoEm).HasColumnName("ATUALIZADO_EM");
        b.Ignore(x => x.PossuiResultado);
        b.HasIndex(x => x.PetId).HasDatabaseName("IDX_EXAMES_PET_ID");
        b.HasOne(x => x.Pet)
            .WithMany(p => p.Exames)
            .HasForeignKey(x => x.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("USUARIOS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("ID");
        b.Property(x => x.Nome).HasColumnName("NOME").IsRequired().HasMaxLength(200);
        b.Property(x => x.Email).HasColumnName("EMAIL").IsRequired().HasMaxLength(200);
        b.Property(x => x.SenhaHash).HasColumnName("SENHA_HASH").IsRequired().HasMaxLength(500);
        b.Property(x => x.Role).HasColumnName("ROLE").IsRequired().HasMaxLength(50);
        b.Property(x => x.Ativo).HasColumnName("ATIVO").HasDefaultValue(true);
        b.Property(x => x.CriadoEm).HasColumnName("CRIADO_EM");
        b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IDX_USUARIOS_EMAIL");
    }
}
