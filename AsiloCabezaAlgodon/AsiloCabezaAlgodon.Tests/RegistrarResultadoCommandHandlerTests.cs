using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Examenes.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    public class RegistrarResultadoCommandHandlerTests
    {
        private readonly IVisitaMedicaRepository _repo = Substitute.For<IVisitaMedicaRepository>();

        [Fact] // LAB-06: registrar dos veces el resultado no debe cargar dos veces el costo
        public async Task Resultado_repetido_se_rechaza_y_no_carga_el_costo_otra_vez()
        {
            var e = new ExamenSolicitado { IdExamenSolicitado = 1, IdVisitaMedica = 10, Estado = "ResultadoRegistrado", Costo = 75m };
            _repo.ObtenerExamenPorIdAsync(1).Returns(e);
            var handler = new RegistrarResultadoCommandHandler(_repo, new SinTransaccion());

            await Assert.ThrowsAsync<ConflictoException>(() =>
                handler.Handle(new RegistrarResultadoCommand(1, "Valores dentro de rango", 75m), CancellationToken.None));

            await _repo.DidNotReceive().CargarCostoAFamiliarAsync(Arg.Any<int>(), Arg.Any<decimal>());
            await _repo.DidNotReceive().ActualizarExamenAsync(Arg.Any<ExamenSolicitado>());
        }

        [Theory] // LAB-03: resultado vacío o costo negativo se rechazan
        [InlineData("", "75")]
        [InlineData("Valores dentro de rango", "-10")]
        public async Task Datos_invalidos_se_rechazan_y_el_examen_sigue_pendiente(string resultado, string costo)
        {
            var e = new ExamenSolicitado { IdExamenSolicitado = 2, IdVisitaMedica = 10, Estado = "Pendiente" };
            _repo.ObtenerExamenPorIdAsync(2).Returns(e);
            var handler = new RegistrarResultadoCommandHandler(_repo, new SinTransaccion());

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new RegistrarResultadoCommand(2, resultado, decimal.Parse(costo)), CancellationToken.None));

            Assert.Equal("Pendiente", e.Estado);
            await _repo.DidNotReceive().ActualizarExamenAsync(Arg.Any<ExamenSolicitado>());
        }

        [Fact] // LAB-02: registro normal
        public async Task Resultado_valido_se_registra_y_carga_el_costo_una_vez()
        {
            var e = new ExamenSolicitado { IdExamenSolicitado = 3, IdVisitaMedica = 10, Estado = "Pendiente" };
            _repo.ObtenerExamenPorIdAsync(3).Returns(e);
            var handler = new RegistrarResultadoCommandHandler(_repo, new SinTransaccion());

            var exito = await handler.Handle(new RegistrarResultadoCommand(3, "Valores dentro de rango", 75m), CancellationToken.None);

            Assert.True(exito);
            Assert.Equal("ResultadoRegistrado", e.Estado);
            await _repo.Received(1).CargarCostoAFamiliarAsync(10, 75m);
        }

        [Fact] // ROB-03: el resultado y el cargo se guardan solo dentro de la transacción
        public async Task Resultado_y_cargo_se_guardan_dentro_de_la_transaccion()
        {
            var e = new ExamenSolicitado { IdExamenSolicitado = 4, IdVisitaMedica = 10, Estado = "Pendiente" };
            _repo.ObtenerExamenPorIdAsync(4).Returns(e);
            var handler = new RegistrarResultadoCommandHandler(_repo, new TransaccionQueNoEjecuta());

            await handler.Handle(new RegistrarResultadoCommand(4, "Valores dentro de rango", 75m), CancellationToken.None);

            await _repo.DidNotReceive().ActualizarExamenAsync(Arg.Any<ExamenSolicitado>());
            await _repo.DidNotReceive().CargarCostoAFamiliarAsync(Arg.Any<int>(), Arg.Any<decimal>());
        }
    }
}
