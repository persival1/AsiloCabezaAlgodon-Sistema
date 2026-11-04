using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.VisitasMedicas.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Semana 2: validación de datos de la visita (FAR-03 y costo de consulta).
    // Semana 3: solo el especialista asignado registra la visita (AUT-08), y solo una vez,
    // cuando la solicitud está Asignada (EXP-07).
    public class RegistrarVisitaCommandHandlerTests
    {
        private readonly IVisitaMedicaRepository _visitas = Substitute.For<IVisitaMedicaRepository>();
        private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();

        private const int Especialista1 = 5;
        private const int Especialista2 = 6;

        private static RegistrarVisitaCommand Visita(decimal costo, string medicamento, int idEspecialista = Especialista1) => new(
            IdSolicitud: 1, Diagnostico: "Lumbalgia mecánica", Observaciones: "Reposo relativo",
            CostoConsulta: costo,
            Examenes: new List<ExamenInput> { new("Hemograma") },
            Prescripciones: new List<PrescripcionInput> { new(medicamento, "50 mg", "Cada 12 horas por 5 días") },
            IdEspecialista: idEspecialista);

        // Solicitud 1, asignada al Especialista 1, en el estado indicado.
        private Solicitud SolicitudEnEstado(string estado)
        {
            var s = new Solicitud { IdSolicitud = 1, Estado = estado, IdEspecialista = estado == "Pendiente" ? null : Especialista1 };
            _solicitudes.ObtenerPorIdAsync(1).Returns(s);
            return s;
        }

        private async Task NoSeCreoNadaNiSeCobro()
        {
            await _visitas.DidNotReceive().CrearAsync(Arg.Any<VisitaMedica>());
            await _visitas.DidNotReceive().CargarCostoAFamiliarAsync(Arg.Any<int>(), Arg.Any<decimal>());
            await _solicitudes.DidNotReceive().ActualizarAsync(Arg.Any<Solicitud>());
        }

        [Fact]
        public async Task Costo_de_consulta_negativo_se_rechaza()
        {
            SolicitudEnEstado("Asignada");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(Visita(-150m, "Diclofenaco"), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
        }

        [Fact] // FAR-03: prescripción sin nombre de medicamento
        public async Task Prescripcion_sin_nombre_se_rechaza()
        {
            SolicitudEnEstado("Asignada");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(Visita(150m, ""), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
        }

        [Fact] // AUT-08: el Especialista 2 no puede registrar la visita de una solicitud del Especialista 1
        public async Task Visita_de_otro_especialista_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Asignada");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
                handler.Handle(Visita(150m, "Diclofenaco", idEspecialista: Especialista2), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
            Assert.Equal("Asignada", solicitud.Estado);
        }

        [Fact] // EXP-07: visita sobre una solicitud que todavía no fue asignada
        public async Task Visita_sobre_solicitud_pendiente_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Pendiente");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<ConflictoException>(() =>
                handler.Handle(Visita(150m, "Diclofenaco"), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
            Assert.Equal("Pendiente", solicitud.Estado);
        }

        [Fact] // EXP-07: segunda visita sobre una solicitud ya atendida (cobraría la consulta otra vez)
        public async Task Segunda_visita_sobre_solicitud_atendida_se_rechaza_y_no_cobra()
        {
            SolicitudEnEstado("Atendida");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<ConflictoException>(() =>
                handler.Handle(Visita(150m, "Diclofenaco"), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
        }

        [Fact]
        public async Task Solicitud_inexistente_se_rechaza()
        {
            _solicitudes.ObtenerPorIdAsync(1).Returns((Solicitud?)null);
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(Visita(150m, "Diclofenaco"), CancellationToken.None));

            await NoSeCreoNadaNiSeCobro();
        }

        [Fact] // EXP-08: la visita correcta se crea, cobra la consulta una vez y deja la solicitud Atendida
        public async Task Visita_del_especialista_asignado_se_registra_y_cobra_una_vez()
        {
            var solicitud = SolicitudEnEstado("Asignada");
            _visitas.CrearAsync(Arg.Any<VisitaMedica>()).Returns(llamada =>
            {
                var v = llamada.Arg<VisitaMedica>();
                v.IdVisitaMedica = 50;
                return v;
            });
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new SinTransaccion());

            var idVisita = await handler.Handle(Visita(150m, "Diclofenaco"), CancellationToken.None);

            Assert.Equal(50, idVisita);
            Assert.Equal("Atendida", solicitud.Estado);
            await _visitas.Received(1).CargarCostoAFamiliarAsync(50, 150m);
        }

        [Fact] // ROB-03: los tres guardados de la visita van dentro de la transacción
        public async Task Visita_cargo_y_estado_se_guardan_dentro_de_la_transaccion()
        {
            SolicitudEnEstado("Asignada");
            var handler = new RegistrarVisitaCommandHandler(_visitas, _solicitudes, new TransaccionQueNoEjecuta());

            await handler.Handle(Visita(150m, "Diclofenaco"), CancellationToken.None);

            await NoSeCreoNadaNiSeCobro();
        }
    }
}
