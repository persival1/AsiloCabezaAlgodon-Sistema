using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiloCabezaAlgodon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Internos",
                columns: table => new
                {
                    IdInterno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MotivoIngreso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Internos", x => x.IdInterno);
                });

            migrationBuilder.CreateTable(
                name: "Familiares",
                columns: table => new
                {
                    IdFamiliar = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parentesco = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuotaMensual = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IdInterno = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familiares", x => x.IdFamiliar);
                    table.ForeignKey(
                        name: "FK_Familiares_Internos_IdInterno",
                        column: x => x.IdInterno,
                        principalTable: "Internos",
                        principalColumn: "IdInterno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicamentosCajon",
                columns: table => new
                {
                    IdMedicamentoCajon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosis = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frecuencia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdInterno = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicamentosCajon", x => x.IdMedicamentoCajon);
                    table.ForeignKey(
                        name: "FK_MedicamentosCajon_Internos_IdInterno",
                        column: x => x.IdInterno,
                        principalTable: "Internos",
                        principalColumn: "IdInterno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Psicopatologias",
                columns: table => new
                {
                    IdPsicopatologia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaDiagnostico = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdInterno = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Psicopatologias", x => x.IdPsicopatologia);
                    table.ForeignKey(
                        name: "FK_Psicopatologias_Internos_IdInterno",
                        column: x => x.IdInterno,
                        principalTable: "Internos",
                        principalColumn: "IdInterno",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdInterno = table.Column<int>(type: "int", nullable: false),
                    IdMedicoGeneral = table.Column<int>(type: "int", nullable: false),
                    IdEspecialista = table.Column<int>(type: "int", nullable: true),
                    IdEnfermero = table.Column<int>(type: "int", nullable: true),
                    FechaHoraCita = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Internos_IdInterno",
                        column: x => x.IdInterno,
                        principalTable: "Internos",
                        principalColumn: "IdInterno",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_IdEnfermero",
                        column: x => x.IdEnfermero,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_IdEspecialista",
                        column: x => x.IdEspecialista,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_IdMedicoGeneral",
                        column: x => x.IdMedicoGeneral,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Familiares_IdInterno",
                table: "Familiares",
                column: "IdInterno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicamentosCajon_IdInterno",
                table: "MedicamentosCajon",
                column: "IdInterno");

            migrationBuilder.CreateIndex(
                name: "IX_Psicopatologias_IdInterno",
                table: "Psicopatologias",
                column: "IdInterno");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_IdEnfermero",
                table: "Solicitudes",
                column: "IdEnfermero");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_IdEspecialista",
                table: "Solicitudes",
                column: "IdEspecialista");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_IdInterno",
                table: "Solicitudes",
                column: "IdInterno");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_IdMedicoGeneral",
                table: "Solicitudes",
                column: "IdMedicoGeneral");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Familiares");

            migrationBuilder.DropTable(
                name: "MedicamentosCajon");

            migrationBuilder.DropTable(
                name: "Psicopatologias");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Internos");
        }
    }
}
