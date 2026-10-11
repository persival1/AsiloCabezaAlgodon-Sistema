using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiloCabezaAlgodon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCostosYReportes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoConsulta",
                table: "VisitasMedicas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Costo",
                table: "ExamenesSolicitados",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoConsulta",
                table: "VisitasMedicas");

            migrationBuilder.DropColumn(
                name: "Costo",
                table: "ExamenesSolicitados");
        }
    }
}
