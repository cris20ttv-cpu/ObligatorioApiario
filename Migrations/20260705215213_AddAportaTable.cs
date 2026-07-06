using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ObligatorioApiario.Migrations
{
    /// <inheritdoc />
    public partial class AddAportaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aporta",
                columns: table => new
                {
                    ID_Aporte = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ID_Colmena = table.Column<int>(type: "integer", nullable: false),
                    ID_Barril = table.Column<int>(type: "integer", nullable: false),
                    ID_Cosecha = table.Column<int>(type: "integer", nullable: false),
                    Kilos = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aporta", x => x.ID_Aporte);
                    table.ForeignKey(
                        name: "FK_Aporta_Barriles_ID_Barril",
                        column: x => x.ID_Barril,
                        principalTable: "Barriles",
                        principalColumn: "ID_Barril",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aporta_Colmenas_ID_Colmena",
                        column: x => x.ID_Colmena,
                        principalTable: "Colmenas",
                        principalColumn: "ID_Colmena",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aporta_Cosechas_ID_Cosecha",
                        column: x => x.ID_Cosecha,
                        principalTable: "Cosechas",
                        principalColumn: "ID_Cosecha",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aporta_ID_Barril",
                table: "Aporta",
                column: "ID_Barril");

            migrationBuilder.CreateIndex(
                name: "IX_Aporta_ID_Colmena",
                table: "Aporta",
                column: "ID_Colmena");

            migrationBuilder.CreateIndex(
                name: "IX_Aporta_ID_Cosecha",
                table: "Aporta",
                column: "ID_Cosecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aporta");
        }
    }
}
