using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObligatorioApiario.Migrations
{
    /// <inheritdoc />
    public partial class AddTareasVisita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TareasVisita",
                columns: table => new
                {
                    ID_Tarea = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ID_Visita = table.Column<int>(type: "integer", nullable: false),
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareasVisita", x => x.ID_Tarea);
                    table.ForeignKey(
                        name: "FK_TareasVisita_Colmenas_ID_Colmena",
                        column: x => x.ID_Colmena,
                        principalTable: "Colmenas",
                        principalColumn: "ID_Colmena",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TareasVisita_Visitas_ID_Visita",
                        column: x => x.ID_Visita,
                        principalTable: "Visitas",
                        principalColumn: "ID_Visita",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TareasVisita_ID_Colmena",
                table: "TareasVisita",
                column: "ID_Colmena");

            migrationBuilder.CreateIndex(
                name: "IX_TareasVisita_ID_Visita",
                table: "TareasVisita",
                column: "ID_Visita");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TareasVisita");
        }
    }
}
