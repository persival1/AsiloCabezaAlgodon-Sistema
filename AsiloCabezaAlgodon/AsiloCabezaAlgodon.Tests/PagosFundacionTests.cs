using AsiloCabezaAlgodon.Application.Caja.PagosFundacion;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Reportes;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // AC-07 (solicitud SQA-ACM-01): registro de pagos del asilo a la fundación.
    // Escenario: se le deben Q595.00 por cargos médicos y ya se le pagaron Q200.00.
    public class PagosFundacionTests
    {
        private readonly ICajaRepository _caja = Substitute.For<ICajaRepository>();

        public PagosFundacionTests()
        {
            _caja.TotalCargosMedicosAsync().Returns(595m);
            _caja.ObtenerPagosFundacionAsync().Returns(new List<PagoFundacion> { new() { IdPagoFundacion = 1, Monto = 200m, NumeroComprobante = "R-001", Fecha = new DateTime(2026, 11, 2) } });
            _caja.CrearPagoFundacionAsync(Arg.Any<PagoFundacion>()).Returns(c => c.Arg<PagoFundacion>());
        }

        [Fact]
        public async Task Un_pago_valido_se_registra()
        {
            await new RegistrarPagoFundacionCommandHandler(_caja).Handle(new RegistrarPagoFundacionCommand(395m, "R-002", "Saldo de noviembre"), CancellationToken.None);
            await _caja.Received(1).CrearPagoFundacionAsync(Arg.Is<PagoFundacion>(p => p.Monto == 395m && p.NumeroComprobante == "R-002"));
        }

        [Fact]
        public async Task No_se_puede_pagar_mas_de_lo_adeudado()
        {
            var ex = await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                new RegistrarPagoFundacionCommandHandler(_caja).Handle(new RegistrarPagoFundacionCommand(400m, "R-002", null), CancellationToken.None));
            Assert.Contains("Q395.00", ex.Message);
            await _caja.DidNotReceive().CrearPagoFundacionAsync(Arg.Any<PagoFundacion>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task Un_monto_cero_o_negativo_se_rechaza(decimal monto)
        {
            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                new RegistrarPagoFundacionCommandHandler(_caja).Handle(new RegistrarPagoFundacionCommand(monto, "R-002", null), CancellationToken.None));
        }

        [Fact]
        public async Task El_comprobante_es_obligatorio()
        {
            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                new RegistrarPagoFundacionCommandHandler(_caja).Handle(new RegistrarPagoFundacionCommand(100m, "  ", null), CancellationToken.None));
        }

        [Fact]
        public async Task El_informe_4_separa_lo_pagado_de_lo_pendiente()
        {
            var visitas = Substitute.For<IVisitaMedicaRepository>();
            visitas.ObtenerTodasAsync().Returns(new List<VisitaMedica>());
            var datos = await new ReportePagosFundacionQueryHandler(visitas, _caja).Handle(new ReportePagosFundacionQuery(null, null), CancellationToken.None);

            Assert.Equal(595m, datos.AdeudadoAcumulado);
            Assert.Equal(200m, datos.PagadoAcumulado);
            Assert.Equal(395m, datos.SaldoPendiente);
            Assert.Equal(200m, datos.TotalPagado);
            Assert.Single(datos.Pagos);
        }
    }
}
