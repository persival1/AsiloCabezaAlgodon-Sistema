using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Solicitudes.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Semana 3: validar la asignación (EXP-06), no reasignar una solicitud atendida (EXP-07)
    // y decir la verdad sobre el correo al familiar (COR-02, COR-03, COR-04).
    public class AsignarMedicoCommandHandlerTests
    {
        private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();
        private readonly IUsuarioRepository _usuarios = Substitute.For<IUsuarioRepository>();
        private readonly IEmailService _correo = Substitute.For<IEmailService>();

        private const int Especialista = 5;
        private const int Enfermero = 7;
        private const int Farmacia = 9;
        private const int Laboratorio = 10;

        public AsignarMedicoCommandHandlerTests()
        {
            _usuarios.TieneRolAsync(Especialista, "Especialista").Returns(true);
            _usuarios.TieneRolAsync(Enfermero, "Enfermero").Returns(true);
            _correo.EnviarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        }

        private AsignarMedicoCommandHandler Handler() => new(_solicitudes, _usuarios, _correo);

        private Solicitud SolicitudEnEstado(string estado, string correoFamiliar = "familiar@ejemplo.test")
        {
            var s = new Solicitud
            {
                IdSolicitud = 1,
                Estado = estado,
                Motivo = "Dolor lumbar",
                Interno = new Interno { Nombres = "Juan", Apellidos = "Pérez", Familiar = new Familiar { Correo = correoFamiliar } }
            };
            _solicitudes.ObtenerPorIdAsync(1).Returns(s);
            return s;
        }

        private static AsignarMedicoCommand Asignacion(int idEspecialista = Especialista, int? idEnfermero = Enfermero, DateTime? fecha = null) =>
            new(1, idEspecialista, idEnfermero, fecha ?? DateTime.Now.AddDays(5));

        [Fact] // EXP-05: asignación correcta
        public async Task Asignacion_valida_queda_Asignada_y_avisa_que_se_envio_el_correo()
        {
            var solicitud = SolicitudEnEstado("Pendiente");

            var resultado = await Handler().Handle(Asignacion(), CancellationToken.None);

            Assert.NotNull(resultado);
            Assert.True(resultado.CorreoEnviado);
            Assert.Equal("Asignada", solicitud.Estado);
            Assert.Equal(Especialista, solicitud.IdEspecialista);
        }

        [Fact] // EXP-07: no se reasigna una solicitud que ya fue atendida
        public async Task Reasignar_una_solicitud_atendida_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Atendida");

            await Assert.ThrowsAsync<ConflictoException>(() => Handler().Handle(Asignacion(), CancellationToken.None));

            Assert.Equal("Atendida", solicitud.Estado);
            await _solicitudes.DidNotReceive().ActualizarAsync(Arg.Any<Solicitud>());
        }

        [Fact] // EXP-06: fecha de cita en el pasado
        public async Task Fecha_de_cita_en_el_pasado_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Pendiente");

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                Handler().Handle(Asignacion(fecha: new DateTime(2020, 1, 1, 9, 0, 0)), CancellationToken.None));

            Assert.Equal("Pendiente", solicitud.Estado);
            await _solicitudes.DidNotReceive().ActualizarAsync(Arg.Any<Solicitud>());
        }

        [Fact] // EXP-06: un usuario de Farmacia como especialista
        public async Task Especialista_que_no_tiene_ese_rol_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Pendiente");

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                Handler().Handle(Asignacion(idEspecialista: Farmacia), CancellationToken.None));

            Assert.Equal("Pendiente", solicitud.Estado);
            await _solicitudes.DidNotReceive().ActualizarAsync(Arg.Any<Solicitud>());
        }

        [Fact] // EXP-06: un usuario de Laboratorio como enfermero
        public async Task Enfermero_que_no_tiene_ese_rol_se_rechaza()
        {
            var solicitud = SolicitudEnEstado("Pendiente");

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                Handler().Handle(Asignacion(idEnfermero: Laboratorio), CancellationToken.None));

            Assert.Equal("Pendiente", solicitud.Estado);
            await _solicitudes.DidNotReceive().ActualizarAsync(Arg.Any<Solicitud>());
        }

        [Fact] // El enfermero es opcional (supuesto 2)
        public async Task Asignacion_sin_enfermero_es_valida()
        {
            var solicitud = SolicitudEnEstado("Pendiente");

            var resultado = await Handler().Handle(Asignacion(idEnfermero: null), CancellationToken.None);

            Assert.NotNull(resultado);
            Assert.Equal("Asignada", solicitud.Estado);
        }

        [Fact] // COR-02 y COR-04: si el correo falla, la asignación se conserva y se informa que no se envió
        public async Task Si_el_correo_falla_la_asignacion_se_conserva_y_se_informa()
        {
            var solicitud = SolicitudEnEstado("Pendiente");
            _correo.EnviarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var resultado = await Handler().Handle(Asignacion(), CancellationToken.None);

            Assert.NotNull(resultado);
            Assert.False(resultado.CorreoEnviado);
            Assert.Equal("Asignada", solicitud.Estado);
            await _solicitudes.Received(1).ActualizarAsync(solicitud);
        }

        [Fact] // Si el familiar no tiene correo, no se dice que se envió
        public async Task Familiar_sin_correo_informa_que_no_se_envio()
        {
            SolicitudEnEstado("Pendiente", correoFamiliar: "");

            var resultado = await Handler().Handle(Asignacion(), CancellationToken.None);

            Assert.NotNull(resultado);
            Assert.False(resultado.CorreoEnviado);
        }

        [Fact]
        public async Task Solicitud_inexistente_devuelve_null()
        {
            _solicitudes.ObtenerPorIdAsync(99999).Returns((Solicitud?)null);

            var resultado = await Handler().Handle(new AsignarMedicoCommand(99999, Especialista, null, DateTime.Now.AddDays(5)), CancellationToken.None);

            Assert.Null(resultado);
        }
    }
}
