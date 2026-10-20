using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace VetSync.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("VetSync.Domain.Entities.Tutor", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<string>("Nome").IsRequired().HasMaxLength(200).HasColumnName("NOME");
                b.Property<string>("Email").IsRequired().HasMaxLength(200).HasColumnName("EMAIL");
                b.Property<string>("Telefone").HasMaxLength(20).HasColumnName("TELEFONE");
                b.Property<string>("Cpf").HasMaxLength(14).HasColumnName("CPF");
                b.Property<string>("Endereco").HasMaxLength(500).HasColumnName("ENDERECO");
                b.Property<bool>("Ativo").HasDefaultValue(true).HasColumnName("ATIVO");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.Property<DateTime?>("AtualizadoEm").HasColumnName("ATUALIZADO_EM");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique().HasDatabaseName("IDX_TUTORES_EMAIL");
                b.HasIndex("Cpf").IsUnique().HasDatabaseName("IDX_TUTORES_CPF");
                b.ToTable("TUTORES");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Pet", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<string>("Nome").IsRequired().HasMaxLength(100).HasColumnName("NOME");
                b.Property<string>("Especie").IsRequired().HasMaxLength(50).HasColumnName("ESPECIE");
                b.Property<string>("Raca").HasMaxLength(100).HasColumnName("RACA");
                b.Property<string>("Sexo").HasMaxLength(10).HasColumnName("SEXO");
                b.Property<DateTime?>("DataNascimento").HasColumnName("DATA_NASCIMENTO");
                b.Property<decimal?>("Peso").HasColumnType("NUMBER(8,2)").HasColumnName("PESO");
                b.Property<string>("Cor").HasMaxLength(50).HasColumnName("COR");
                b.Property<bool>("Ativo").HasDefaultValue(true).HasColumnName("ATIVO");
                b.Property<int>("TutorId").HasColumnName("TUTOR_ID");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.Property<DateTime?>("AtualizadoEm").HasColumnName("ATUALIZADO_EM");
                b.HasKey("Id");
                b.HasIndex("Especie").HasDatabaseName("IDX_PETS_ESPECIE");
                b.HasIndex("TutorId").HasDatabaseName("IDX_PETS_TUTOR_ID");
                b.ToTable("PETS");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Consulta", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<int>("PetId").HasColumnName("PET_ID");
                b.Property<string>("Veterinario").IsRequired().HasMaxLength(200).HasColumnName("VETERINARIO");
                b.Property<DateTime>("DataConsulta").HasColumnName("DATA_CONSULTA");
                b.Property<string>("Motivo").HasMaxLength(2000).HasColumnName("MOTIVO");
                b.Property<string>("Diagnostico").HasMaxLength(2000).HasColumnName("DIAGNOSTICO");
                b.Property<string>("Prescricao").HasMaxLength(2000).HasColumnName("PRESCRICAO");
                b.Property<string>("Status").IsRequired().HasMaxLength(50).HasDefaultValue("Agendada").HasColumnName("STATUS");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.Property<DateTime?>("AtualizadoEm").HasColumnName("ATUALIZADO_EM");
                b.HasKey("Id");
                b.HasIndex("Status").HasDatabaseName("IDX_CONSULTAS_STATUS");
                b.HasIndex("PetId").HasDatabaseName("IDX_CONSULTAS_PET_ID");
                b.ToTable("CONSULTAS");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Vacinacao", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<int>("PetId").HasColumnName("PET_ID");
                b.Property<string>("Vacina").IsRequired().HasMaxLength(200).HasColumnName("VACINA");
                b.Property<string>("Fabricante").HasMaxLength(200).HasColumnName("FABRICANTE");
                b.Property<string>("Lote").HasMaxLength(100).HasColumnName("LOTE");
                b.Property<DateTime>("DataAplicacao").HasColumnName("DATA_APLICACAO");
                b.Property<DateTime?>("ProximaDose").HasColumnName("PROXIMA_DOSE");
                b.Property<string>("Veterinario").HasMaxLength(200).HasColumnName("VETERINARIO");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.HasKey("Id");
                b.HasIndex("PetId").HasDatabaseName("IDX_VACINACOES_PET_ID");
                b.ToTable("VACINACOES");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Exame", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<int>("PetId").HasColumnName("PET_ID");
                b.Property<string>("TipoExame").IsRequired().HasMaxLength(200).HasColumnName("TIPO_EXAME");
                b.Property<DateTime>("DataExame").HasColumnName("DATA_EXAME");
                b.Property<string>("Resultado").HasMaxLength(2000).HasColumnName("RESULTADO");
                b.Property<string>("Laboratorio").HasMaxLength(200).HasColumnName("LABORATORIO");
                b.Property<string>("Veterinario").HasMaxLength(200).HasColumnName("VETERINARIO");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.Property<DateTime?>("AtualizadoEm").HasColumnName("ATUALIZADO_EM");
                b.HasKey("Id");
                b.HasIndex("PetId").HasDatabaseName("IDX_EXAMES_PET_ID");
                b.ToTable("EXAMES");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Usuario", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("NUMBER(10)").HasColumnName("ID");
                b.Property<string>("Nome").IsRequired().HasMaxLength(200).HasColumnName("NOME");
                b.Property<string>("Email").IsRequired().HasMaxLength(200).HasColumnName("EMAIL");
                b.Property<string>("SenhaHash").IsRequired().HasMaxLength(500).HasColumnName("SENHA_HASH");
                b.Property<string>("Role").IsRequired().HasMaxLength(50).HasColumnName("ROLE");
                b.Property<bool>("Ativo").HasDefaultValue(true).HasColumnName("ATIVO");
                b.Property<DateTime>("CriadoEm").HasColumnName("CRIADO_EM");
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique().HasDatabaseName("IDX_USUARIOS_EMAIL");
                b.ToTable("USUARIOS");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Pet", b =>
            {
                b.HasOne("VetSync.Domain.Entities.Tutor", "Tutor")
                    .WithMany("Pets")
                    .HasForeignKey("TutorId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Tutor");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Consulta", b =>
            {
                b.HasOne("VetSync.Domain.Entities.Pet", "Pet")
                    .WithMany("Consultas")
                    .HasForeignKey("PetId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Pet");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Vacinacao", b =>
            {
                b.HasOne("VetSync.Domain.Entities.Pet", "Pet")
                    .WithMany("Vacinacoes")
                    .HasForeignKey("PetId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Pet");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Exame", b =>
            {
                b.HasOne("VetSync.Domain.Entities.Pet", "Pet")
                    .WithMany("Exames")
                    .HasForeignKey("PetId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Pet");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Pet", b =>
            {
                b.Navigation("Consultas");
                b.Navigation("Exames");
                b.Navigation("Vacinacoes");
            });

            modelBuilder.Entity("VetSync.Domain.Entities.Tutor", b =>
            {
                b.Navigation("Pets");
            });
#pragma warning restore 612, 618
        }
    }
}
