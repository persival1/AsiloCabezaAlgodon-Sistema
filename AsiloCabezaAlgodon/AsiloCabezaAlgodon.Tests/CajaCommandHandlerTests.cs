using AsiloCabezaAlgodon.Application.Caja.Cobros;
using AsiloCabezaAlgodon.Application.Caja.Donaciones;
using AsiloCabezaAlgodon.Application.Caja.Gastos;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Cobros, donaciones y gastos (casos FIN-05 y FIN-06)
    public class CajaCommandHandlerTests
    {
        private readonly ICajaRepository _repo = Substitute.For<ICajaRepository>();

        public CajaCommandHandlerTests()
        {
            _repo.ExisteFamiliarAsync(1).Returns(true);
            _repo.CrearCobroAsync(Arg.Any<Cobro>()).Returns(c => c.Arg<Cobro>());
        }

        [Theory] // FIN-05: montos inválidos en cobros
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("100000000")]
        public async Task Cobro_con_monto_invalido_se_rechaza(string monto)
        {
            var handler = new CrearCobroCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearCobroCommand(1, "Cuota mensual", decimal.Parse(monto)), CancellationToken.None));

            await _repo.DidNotReceive().CrearCobroAsync(Arg.Any<Cobro>());
        }

        [Fact] // FIN-06: un cobro no puede ligarse a la visita de otro interno
        public async Task Cobro_ligado_a_visita_de_otro_interno_se_rechaza()
        {
            _repo.VisitaEsDelFamiliarAsync(20, 1).Returns(false);
            var handler = new CrearCobroCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearCobroCommand(1, "Cargo médico", 100m, 20), CancellationToken.None));

            await _repo.DidNotReceive().CrearCobroAsync(Arg.Any<Cobro>());
        }

        [Fact]
        public async Task Cobro_a_familiar_inexistente_se_rechaza()
        {
            _repo.ExisteFamiliarAsync(99999).Returns(false);
            var handler = new CrearCobroCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearCobroCommand(99999, "Cuota mensual", 100m), CancellationToken.None));
        }

        [Fact] // FIN-01 y FIN-02: cobro válido, con y sin visita
        public async Task Cobro_valido_se_guarda()
        {
            _repo.VisitaEsDelFamiliarAsync(10, 1).Returns(true);
            _repo.PendienteDeVisitaAsync(10).Returns(265m);
            var handler = new CrearCobroCommandHandler(_repo);

            await handler.Handle(new CrearCobroCommand(1, "Cuota mensual", 500m), CancellationToken.None);
            await handler.Handle(new CrearCobroCommand(1, "Cargo médico", 150m, 10), CancellationToken.None);

            await _repo.Received(2).CrearCobroAsync(Arg.Any<Cobro>());
        }

        [Fact] // AC-01: no se puede pagar más de lo que falta de la visita
        public async Task Cobro_mayor_a_lo_pendiente_de_la_visita_se_rechaza()
        {
            _repo.VisitaEsDelFamiliarAsync(10, 1).Returns(true);
            _repo.PendienteDeVisitaAsync(10).Returns(115m);
            var handler = new CrearCobroCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearCobroCommand(1, "Cargo médico", 150m, 10), CancellationToken.None));
            await _repo.DidNotReceive().CrearCobroAsync(Arg.Any<Cobro>());
        }

        [Fact] // AC-01: pagar exactamente lo pendiente sí se acepta
        public async Task Cobro_igual_a_lo_pendiente_se_acepta()
        {
            _repo.VisitaEsDelFamiliarAsync(10, 1).Returns(true);
            _repo.PendienteDeVisitaAsync(10).Returns(115m);
            var handler = new CrearCobroCommandHandler(_repo);

            await handler.Handle(new CrearCobroCommand(1, "Cargo médico", 115m, 10), CancellationToken.None);
            await _repo.Received(1).CrearCobroAsync(Arg.Any<Cobro>());
        }

        [Fact] // FIN-05: donación con monto negativo
        public async Task Donacion_con_monto_negativo_se_rechaza()
        {
            var handler = new CrearDonacionCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearDonacionCommand("Particular", "Juan Pérez", -100m, ""), CancellationToken.None));

            await _repo.DidNotReceive().CrearDonacionAsync(Arg.Any<Donacion>());
        }

        [Fact] // FIN-05: gasto con monto negativo
        public async Task Gasto_con_monto_negativo_se_rechaza()
        {
            var handler = new CrearGastoCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new CrearGastoCommand("Energía eléctrica", -350m), CancellationToken.None));

            await _repo.DidNotReceive().CrearGastoAsync(Arg.Any<Gasto>());
        }
    }
}
