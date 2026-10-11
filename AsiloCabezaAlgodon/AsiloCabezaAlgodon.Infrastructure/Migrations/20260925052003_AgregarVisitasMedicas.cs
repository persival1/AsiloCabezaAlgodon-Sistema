using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiloCabezaAlgodon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVisitasMedicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisitasMedicas",
                columns: table => new
                {
                    IdVisitaMedica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Diagnostico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaVisita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitasMedicas", x => x.IdVisitaMedica);
                    table.ForeignKey(
                        name: "FK_VisitasMedicas_Solicitudes_IdSolicitud",
                        column: x => x.IdSolicitud,
                        principalTable: "Solicitudes",
                        principalColumn: "IdSolicitud",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamenesSolicitados",
                columns: table => new
                {
                    IdExamenSolicitado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreExamen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaResultado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdVisitaMedica = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamenesSolicitados", x => x.IdExamenSolicitado);
                    table.ForeignKey(
                        name: "FK_ExamenesSolicitados_VisitasMedicas_IdVisitaMedica",
                        column: x => x.IdVisitaMedica,
                        principalTable: "VisitasMedicas",
                        principalColumn: "IdVisitaMedica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescripciones",
                columns: table => new
                {
                    IdPrescripcion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreMedicamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Indicaciones = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Costo = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    FechaDespacho = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdVisitaMedica = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescripciones", x => x.IdPrescripcion);
                    table.ForeignKey(
                        name: "FK_Prescripciones_VisitasMedicas_IdVisitaMedica",
                        column: x => x.IdVisitaMedica,
                        principalTable: "VisitasMedicas",
                        principalColumn: "IdVisitaMedica",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamenesSolicitados_IdVisitaMedica",
                table: "ExamenesSolicitados",
                column: "IdVisitaMedica");

            migrationBuilder.CreateIndex(
                name: "IX_Prescripciones_IdVisitaMedica",
                table: "Prescripciones",
                column: "IdVisitaMedica");

            migrationBuilder.CreateIndex(
                name: "IX_VisitasMedicas_IdSolicitud",
                table: "VisitasMedicas",
                column: "IdSolicitud");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamenesSolicitados");

            migrationBuilder.DropTable(
                name: "Prescripciones");

            migrationBuilder.DropTable(
                name: "VisitasMedicas");
        }
    }
}
