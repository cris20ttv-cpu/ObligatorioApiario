using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObligatorioApiario.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroFisicoToBarril : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroFisico",
                table: "Barriles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Asignar un número físico inicial a los barriles existentes usando ROW_NUMBER
            migrationBuilder.Sql(@"
                UPDATE ""Barriles""
                SET ""NumeroFisico"" = CTE.RowNum
                FROM (
                    SELECT ""ID_Barril"", ROW_NUMBER() OVER(ORDER BY ""ID_Barril"") as RowNum
                    FROM ""Barriles""
                ) as CTE
                WHERE ""Barriles"".""ID_Barril"" = CTE.""ID_Barril""
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroFisico",
                table: "Barriles");
        }
    }
}
