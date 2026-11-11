using AsiloCabezaAlgodon.Application.Reportes;
using AsiloCabezaAlgodon.Domain.Entidad;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AsiloCabezaAlgodon.Tests.Integracion
{
    // AC-01 (solicitud SQA-ACM-01): regla del saldo del familiar con registros controlados.
    //
    //   saldo = cargos médicos (consulta, exámenes y medicamentos con costo)
    //           - pagos ligados a una visita
    //   positivo = debe; 0 = al día; negativo = pagó de más (saldo a favor)
    //
    // La cuota mensual es un ingreso por la estadía y no cambia el saldo de cargos médicos.
    // Usa SQL Server real en una base aparte (asilo_pruebas_saldos) que se borra en cada prueba.
    [Trait("Tipo", "Integracion")]
    public class SaldoFamiliarTests
    {
        private static readonly string Conexion = BasesDePrueba.Conexion("asilo_pruebas_saldos");

        private static BDConexion NuevoContexto() =>
            new(new DbContextOptionsBuilder<BDConexion>().UseSqlServer(Conexion).Options);

        // Escenario controlado: una visita con consulta Q150, examen Q75 y medicamento Q40 (total Q265),
        // ya cargados al familiar (saldo Q265), igual que lo haría el sistema.
        private static async Task<(int IdFamiliar, int IdVisita)> PrepararAsync()
        {
            await using var ctx = NuevoContexto();
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();

            var familiar = new Familiar { Nombres = "Ana", Apellidos = "Prueba", Parentesco = "Hija", Telefono = "1", Correo = "ana@prueba.test", CuotaMensual = 500, SaldoPendiente = 265 };
            var interno = new Interno { Nombres = "Rosa", Apellidos = "Prueba", MotivoIngreso = "Prueba", FechaNacimiento = new DateTime(1941, 3, 14), FechaIngreso = new DateTime(2025, 6, 2), Familiar = familiar };
            var mg = new Usuario { NombreUsuario = "mg", NombreCompleto = "MG", Rol = new Rol { Nombre = "MedicoGeneral" } };
            var visita = new VisitaMedica
            {
                Diagnostico = "Artrosis", Observaciones = "-", CostoConsulta = 150,
                Solicitud = new Solicitud { Motivo = "Dolor", Estado = "Atendida", Interno = interno, MedicoGeneral = mg },
                ExamenesSolicitados = { new ExamenSolicitado { NombreExamen = "Hemograma", Estado = "ResultadoRegistrado", Costo = 75 } },
                Prescripciones = { new Prescripcion { NombreMedicamento = "Diclofenaco", Dosis = "50 mg", Indicaciones = "-", Estado = "Despachado", Costo = 40 } },
            };
            ctx.VisitasMedicas.Add(visita);
            await ctx.SaveChangesAsync();
            return (familiar.IdFamiliar, visita.IdVisitaMedica);
        }

        private static async Task<decimal> SaldoAsync(int idFamiliar)
        {
            await using var ctx = NuevoContexto();
            return (await ctx.Familiares.FindAsync(idFamiliar))!.SaldoPendiente;
        }

        [Fact]
        public async Task La_cuota_mensual_no_cambia_el_saldo_de_cargos_medicos()
        {
            var (idFamiliar, _) = await PrepararAsync();
            await using (var ctx = NuevoContexto())
                await new CajaRepository(ctx).CrearCobroAsync(new Cobro { IdFamiliar = idFamiliar, Concepto = "Cuota mensual", Monto = 500, Fecha = DateTime.Now });

            Assert.Equal(265m, await SaldoAsync(idFamiliar));
        }

        [Fact]
        public async Task Un_pago_ligado_a_la_visita_baja_el_saldo_y_lo_pendiente()
        {
            var (idFamiliar, idVisita) = await PrepararAsync();
            await using (var ctx = NuevoContexto())
            {
                var repo = new CajaRepository(ctx);
                Assert.Equal(265m, await repo.PendienteDeVisitaAsync(idVisita));
                await repo.CrearCobroAsync(new Cobro { IdFamiliar = idFamiliar, Concepto = "Cargo médico", Monto = 150, Fecha = DateTime.Now, IdVisitaMedica = idVisita });
            }
            await using (var ctx = NuevoContexto())
                Assert.Equal(115m, await new CajaRepository(ctx).PendienteDeVisitaAsync(idVisita));

            Assert.Equal(115m, await SaldoAsync(idFamiliar));
        }

        [Fact]
        public async Task La_conciliacion_cuadra_con_cuota_y_pago_parcial()
        {
            var (idFamiliar, idVisita) = await PrepararAsync();
            await using (var ctx = NuevoContexto())
            {
                var repo = new CajaRepository(ctx);
                await repo.CrearCobroAsync(new Cobro { IdFamiliar = idFamiliar, Concepto = "Cuota mensual", Monto = 500, Fecha = DateTime.Now });
                await repo.CrearCobroAsync(new Cobro { IdFamiliar = idFamiliar, Concepto = "Cargo médico", Monto = 150, Fecha = DateTime.Now, IdVisitaMedica = idVisita });
            }

            await using var c = NuevoContexto();
            var handler = new ReporteConciliacionSaldosQueryHandler(new InternoRepository(c), new VisitaMedicaRepository(c), new CajaRepository(c));
            var fila = Assert.Single(await handler.Handle(new ReporteConciliacionSaldosQuery(), CancellationToken.None));

            Assert.Equal(265m, fila.TotalCargos);
            Assert.Equal(150m, fila.TotalCobros);
            Assert.Equal(115m, fila.SaldoRecalculado);
            Assert.True(fila.Cuadra);
        }
    }
}
