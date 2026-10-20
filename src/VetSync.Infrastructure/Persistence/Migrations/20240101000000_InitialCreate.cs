using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetSync.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240101000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TUTORES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    TELEFONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: true),
                    CPF = table.Column<string>(type: "NVARCHAR2(14)", maxLength: 14, nullable: true),
                    ENDERECO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_TUTORES", x => x.ID));

            migrationBuilder.CreateTable(
                name: "PETS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    ESPECIE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    RACA = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    SEXO = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: true),
                    DATA_NASCIMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    PESO = table.Column<decimal>(type: "NUMBER(8,2)", nullable: true),
                    COR = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: true),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    TUTOR_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PETS", x => x.ID);
                    table.ForeignKey("FK_PETS_TUTORES", x => x.TUTOR_ID, "TUTORES", "ID", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTAS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PET_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VETERINARIO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DATA_CONSULTA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MOTIVO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    DIAGNOSTICO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    PRESCRICAO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false, defaultValue: "Agendada"),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTAS", x => x.ID);
                    table.ForeignKey("FK_CONSULTAS_PETS", x => x.PET_ID, "PETS", "ID", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VACINACOES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PET_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VACINA = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    FABRICANTE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    LOTE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    DATA_APLICACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PROXIMA_DOSE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VETERINARIO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VACINACOES", x => x.ID);
                    table.ForeignKey("FK_VACINACOES_PETS", x => x.PET_ID, "PETS", "ID", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EXAMES",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PET_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TIPO_EXAME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DATA_EXAME = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RESULTADO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: true),
                    LABORATORIO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    VETERINARIO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATUALIZADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EXAMES", x => x.ID);
                    table.ForeignKey("FK_EXAMES_PETS", x => x.PET_ID, "PETS", "ID", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IDX_TUTORES_EMAIL", "TUTORES", "EMAIL", unique: true);
            migrationBuilder.CreateIndex("IDX_TUTORES_CPF", "TUTORES", "CPF", unique: true);
            migrationBuilder.CreateIndex("IDX_PETS_ESPECIE", "PETS", "ESPECIE");
            migrationBuilder.CreateIndex("IDX_PETS_TUTOR_ID", "PETS", "TUTOR_ID");
            migrationBuilder.CreateIndex("IDX_CONSULTAS_STATUS", "CONSULTAS", "STATUS");
            migrationBuilder.CreateIndex("IDX_CONSULTAS_PET_ID", "CONSULTAS", "PET_ID");
            migrationBuilder.CreateIndex("IDX_VACINACOES_PET_ID", "VACINACOES", "PET_ID");
            migrationBuilder.CreateIndex("IDX_EXAMES_PET_ID", "EXAMES", "PET_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("EXAMES");
            migrationBuilder.DropTable("VACINACOES");
            migrationBuilder.DropTable("CONSULTAS");
            migrationBuilder.DropTable("PETS");
            migrationBuilder.DropTable("TUTORES");
        }
    }
}
