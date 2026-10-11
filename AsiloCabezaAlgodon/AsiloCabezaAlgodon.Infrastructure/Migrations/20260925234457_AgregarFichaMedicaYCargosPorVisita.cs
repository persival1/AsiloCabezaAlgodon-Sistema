using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiloCabezaAlgodon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFichaMedicaYCargosPorVisita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdVisitaMedica",
                table: "Cobros",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_IdVisitaMedica",
                table: "Cobros",
                column: "IdVisitaMedica");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobros_VisitasMedicas_IdVisitaMedica",
                table: "Cobros",
                column: "IdVisitaMedica",
                principalTable: "VisitasMedicas",
                principalColumn: "IdVisitaMedica",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobros_VisitasMedicas_IdVisitaMedica",
                table: "Cobros");

            migrationBuilder.DropIndex(
                name: "IX_Cobros_IdVisitaMedica",
                table: "Cobros");

            migrationBuilder.DropColumn(
                name: "Especialidad",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IdVisitaMedica",
                table: "Cobros");
        }
    }
}
