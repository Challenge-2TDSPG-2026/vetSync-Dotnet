using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetSync.Infrastructure.Persistence.Migrations
{
    /// <summary>Sprint 4: tabela de usuários para autenticação JWT.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261017000000_AddUsuarios")]
    public partial class AddUsuarios : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SENHA_HASH = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ROLE = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false, defaultValue: true),
                    CRIADO_EM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.ID);
                });

            migrationBuilder.CreateIndex("IDX_USUARIOS_EMAIL", "USUARIOS", "EMAIL", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "USUARIOS");
        }
    }
}
