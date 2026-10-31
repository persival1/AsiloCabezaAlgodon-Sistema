using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Internos.Comandos;
using AsiloCabezaAlgodon.Application.Solicitudes.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Revisión del 30/10/2026: crear una solicitud y agregar psicopatologías o medicinas de
    // cajón no validaban nada. Con un interno inexistente la base rechazaba el registro y la
    // API respondía 500; con textos vacíos se guardaban registros sin nombre ni motivo.
    public class FichaYSolicitudCommandHandlerTests
    {
        private readonly IInternoRepository _internos = Substitute.For<IInternoRepository>();
        private readonly ISolicitudRepository _solicitudes = Substitute.For<ISolicitudRepository>();

        public FichaYSolicitudCommandHandlerTests()
        {
            _internos.ObtenerPorIdAsync(1).Returns(new Interno { IdInterno = 1 });
            _internos.ObtenerPorIdAsync(99999).Returns((Interno?)null);
            _solicitudes.CrearAsync(Arg.Any<Solicitud>()).Returns(llamada => llamada.Arg<Solicitud>());
            _internos.AgregarPsicopatologiaAsync(Arg.Any<Psicopatologia>()).Returns(llamada => llamada.Arg<Psicopatologia>());
            _internos.AgregarMedicamentoCajonAsync(Arg.Any<MedicamentoCajon>()).Returns(llamada => llamada.Arg<MedicamentoCajon>());
        }

        // ---- Solicitudes (EXP-04) ----

        [Fact]
        public async Task Solicitud_de_un_interno_inexistente_se_rechaza()
        {
            var handler = new CrearSolicitudCommandHandler(_solicitudes, _internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearSolicitudCommand(99999, "Dolor lumbar", "Geriatría", 2), CancellationToken.None));

            await _solicitudes.DidNotReceive().CrearAsync(Arg.Any<Solicitud>());
        }

        [Fact]
        public async Task Solicitud_sin_motivo_se_rechaza()
        {
            var handler = new CrearSolicitudCommandHandler(_solicitudes, _internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearSolicitudCommand(1, "  ", "Geriatría", 2), CancellationToken.None));

            await _solicitudes.DidNotReceive().CrearAsync(Arg.Any<Solicitud>());
        }

        [Fact]
        public async Task Solicitud_valida_se_crea_Pendiente()
        {
            var handler = new CrearSolicitudCommandHandler(_solicitudes, _internos);

            await handler.Handle(new CrearSolicitudCommand(1, "Dolor lumbar", "Geriatría", 2), CancellationToken.None);

            await _solicitudes.Received(1).CrearAsync(Arg.Is<Solicitud>(s => s.Estado == "Pendiente" && s.IdInterno == 1));
        }

        // ---- Psicopatologías (EXP-01) ----

        [Fact]
        public async Task Psicopatologia_de_un_interno_inexistente_se_rechaza()
        {
            var handler = new AgregarPsicopatologiaCommandHandler(_internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new AgregarPsicopatologiaCommand(99999, "Demencia", new DateTime(2025, 1, 1)), CancellationToken.None));

            await _internos.DidNotReceive().AgregarPsicopatologiaAsync(Arg.Any<Psicopatologia>());
        }

        [Fact]
        public async Task Psicopatologia_sin_nombre_se_rechaza()
        {
            var handler = new AgregarPsicopatologiaCommandHandler(_internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new AgregarPsicopatologiaCommand(1, "", new DateTime(2025, 1, 1)), CancellationToken.None));

            await _internos.DidNotReceive().AgregarPsicopatologiaAsync(Arg.Any<Psicopatologia>());
        }

        [Fact]
        public async Task Psicopatologia_con_diagnostico_en_el_futuro_se_rechaza()
        {
            var handler = new AgregarPsicopatologiaCommandHandler(_internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new AgregarPsicopatologiaCommand(1, "Demencia", DateTime.Today.AddDays(10)), CancellationToken.None));

            await _internos.DidNotReceive().AgregarPsicopatologiaAsync(Arg.Any<Psicopatologia>());
        }

        [Fact]
        public async Task Psicopatologia_valida_se_guarda()
        {
            var handler = new AgregarPsicopatologiaCommandHandler(_internos);

            await handler.Handle(new AgregarPsicopatologiaCommand(1, "Demencia", new DateTime(2025, 1, 1)), CancellationToken.None);

            await _internos.Received(1).AgregarPsicopatologiaAsync(Arg.Any<Psicopatologia>());
        }

        // ---- Medicinas de cajón (EXP-02) ----

        [Fact]
        public async Task Medicina_de_cajon_de_un_interno_inexistente_se_rechaza()
        {
            var handler = new AgregarMedicamentoCajonCommandHandler(_internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new AgregarMedicamentoCajonCommand(99999, "Paracetamol", "500 mg", "Cada 8 horas"), CancellationToken.None));

            await _internos.DidNotReceive().AgregarMedicamentoCajonAsync(Arg.Any<MedicamentoCajon>());
        }

        [Fact]
        public async Task Medicina_de_cajon_sin_nombre_se_rechaza()
        {
            var handler = new AgregarMedicamentoCajonCommandHandler(_internos);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new AgregarMedicamentoCajonCommand(1, "", "500 mg", "Cada 8 horas"), CancellationToken.None));

            await _internos.DidNotReceive().AgregarMedicamentoCajonAsync(Arg.Any<MedicamentoCajon>());
        }

        [Fact]
        public async Task Medicina_de_cajon_valida_se_guarda()
        {
            var handler = new AgregarMedicamentoCajonCommandHandler(_internos);

            await handler.Handle(new AgregarMedicamentoCajonCommand(1, "Paracetamol", "500 mg", "Cada 8 horas"), CancellationToken.None);

            await _internos.Received(1).AgregarMedicamentoCajonAsync(Arg.Any<MedicamentoCajon>());
        }
    }
}
