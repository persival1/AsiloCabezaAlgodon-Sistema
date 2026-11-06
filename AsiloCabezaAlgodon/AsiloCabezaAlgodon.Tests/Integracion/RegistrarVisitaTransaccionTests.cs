using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using AsiloCabezaAlgodon.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AsiloCabezaAlgodon.Tests.Integracion
{
    // Plan de pruebas SQA, semana 4, caso ROB-03: registrar una visita hace tres guardados
    // (la visita, el cargo al familiar y el estado de la solicitud). Si el tercero falla, no
    // deben quedar cambios a medias: ni visita, ni cargo, ni cambio de estado.
    //
    // A diferencia de las demás pruebas, estas usan los repositorios reales y SQL Server de
    // verdad, en una base aparte que se borra y se vuelve a crear en cada prueba (nunca la
    // base del sistema; ver BasesDePrueba). En GitHub Actions se excluyen con el filtro
    // Tipo!=Integracion.
    [Trait("Tipo", "Integracion")]
    public class RegistrarVisitaTransaccionTests
    {
        private static readonly string Conexion = BasesDePrueba.Conexion("asilo_pruebas_integracion");

        private static BDConexion NuevoContexto() =>
            new(new DbContextOptionsBuilder<BDConexion>().UseSqlServer(Conexion).Options);

        // Repositorio de solicitudes que falla justo en el último guardado de la visita.
        private class SolicitudRepositoryQueFalla : ISolicitudRepository
        {
            private readonly SolicitudRepository _real;
            public SolicitudRepositoryQueFalla(BDConexion context) { _real = new SolicitudRepository(context); }

            public Task<List<Solicitud>> ObtenerTodasAsync() => _real.ObtenerTodasAsync();
            public Task<Solicitud?> ObtenerPorIdAsync(int id) => _real.ObtenerPorIdAsync(id);
            public Task<Solicitud> CrearAsync(Solicitud solicitud) => _real.CrearAsync(solicitud);
            public Task<bool> ActualizarAsync(Solicitud solicitud) =>
                throw new InvalidOperationException("Falla simulada al actualizar la solicitud.");
        }

        // Base nueva con un interno, su familiar (saldo Q0) y una solicitud Asignada.
        private static async Task<(int IdSolicitud, int IdEspecialista)> PrepararBaseAsync()
        {
            await using var ctx = NuevoContexto();
            await ctx.Database.EnsureDeletedAsync();
            await ctx.Database.EnsureCreatedAsync();

            var medico = new Usuario { NombreUsuario = "mg", NombreCompleto = "Médico de prueba", Rol = new Rol { Nombre = "MedicoGeneral" } };
            var especialista = new Usuario { NombreUsuario = "esp", NombreCompleto = "Especialista de prueba", Rol = new Rol { Nombre = "Especialista" } };
            var interno = new Interno
            {
                Nombres = "Interno", Apellidos = "De Prueba", MotivoIngreso = "Prueba",
                FechaNacimiento = new DateTime(1940, 1, 1), FechaIngreso = new DateTime(2020, 1, 1),
                Familiar = new Familiar { Nombres = "Familiar", Apellidos = "De Prueba", Parentesco = "Hijo", Telefono = "1", Correo = "familiar@prueba.test" }
            };
            var solicitud = new Solicitud
            {
                Motivo = "Control", Estado = "Asignada", FechaHoraCita = DateTime.Now.AddDays(1),
                Interno = interno, MedicoGeneral = medico, Especialista = especialista
            };
            ctx.Solicitudes.Add(solicitud);
            await ctx.SaveChangesAsync();
            return (solicitud.IdSolicitud, especialista.IdUsuario);
        }

        private static RegistrarVisitaCommand Visita(int idSolicitud, int idEspecialista) => new(
            idSolicitud, "Lumbalgia mecánica", "Reposo relativo", 150m,
            new List<ExamenInput> { new("Hemograma") },
            new List<PrescripcionInput> { new("Diclofenaco", "50 mg", "Cada 12 horas") },
            idEspecialista);

        private static async Task<(int Visitas, decimal Saldo, string Estado)> LeerResultadoAsync(int idSolicitud)
        {
            await using var ctx = NuevoContexto();
            return (await ctx.VisitasMedicas.CountAsync(),
                    (await ctx.Familiares.SingleAsync()).SaldoPendiente,
                    (await ctx.Solicitudes.SingleAsync(s => s.IdSolicitud == idSolicitud)).Estado);
        }

        [Fact] // ROB-03: si falla el último guardado, no queda nada a medias
        public async Task Si_falla_el_ultimo_guardado_no_queda_visita_ni_cargo_ni_cambio_de_estado()
        {
            var (idSolicitud, idEspecialista) = await PrepararBaseAsync();

            await using (var ctx = NuevoContexto())
            {
                var handler = new RegistrarVisitaCommandHandler(
                    new VisitaMedicaRepository(ctx), new SolicitudRepositoryQueFalla(ctx), new UnidadDeTrabajo(ctx));

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    handler.Handle(Visita(idSolicitud, idEspecialista), CancellationToken.None));
            }

            var (visitas, saldo, estado) = await LeerResultadoAsync(idSolicitud);
            Assert.Equal(0, visitas);
            Assert.Equal(0m, saldo);
            Assert.Equal("Asignada", estado);
        }

        [Fact] // Control: sin fallas, se guardan las tres cosas
        public async Task Sin_fallas_se_guarda_la_visita_el_cargo_y_el_estado()
        {
            var (idSolicitud, idEspecialista) = await PrepararBaseAsync();

            await using (var ctx = NuevoContexto())
            {
                var handler = new RegistrarVisitaCommandHandler(
                    new VisitaMedicaRepository(ctx), new SolicitudRepository(ctx), new UnidadDeTrabajo(ctx));

                await handler.Handle(Visita(idSolicitud, idEspecialista), CancellationToken.None);
            }

            var (visitas, saldo, estado) = await LeerResultadoAsync(idSolicitud);
            Assert.Equal(1, visitas);
            Assert.Equal(150m, saldo);
            Assert.Equal("Atendida", estado);
        }
    }
}
