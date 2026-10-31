using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Application.Prescripciones.Comandos;
using AsiloCabezaAlgodon.Domain.Entidad;
using NSubstitute;

namespace AsiloCabezaAlgodon.Tests
{
    // Basadas en la propuesta del Anexo B del plan de SQA. Cambio respecto a la propuesta:
    // en lugar de devolver false, el handler lanza una excepción (409 o 400), para que la API
    // pueda decirle al usuario POR QUÉ se rechazó. Lo que se comprueba es lo mismo: que no se
    // cargue el costo dos veces y que no se guarde nada con un costo inválido.
    public class DespacharPrescripcionCommandHandlerTests
    {
        private readonly IVisitaMedicaRepository _repo = Substitute.For<IVisitaMedicaRepository>();

        [Fact] // FAR-05: un despacho repetido no debe cargar de nuevo el costo
        public async Task Despacho_repetido_se_rechaza_y_no_carga_el_costo_otra_vez()
        {
            var p = new Prescripcion { IdPrescripcion = 1, IdVisitaMedica = 10, Estado = "Despachado", Costo = 40m };
            _repo.ObtenerPrescripcionPorIdAsync(1).Returns(p);
            var handler = new DespacharPrescripcionCommandHandler(_repo);

            await Assert.ThrowsAsync<ConflictoException>(() =>
                handler.Handle(new DespacharPrescripcionCommand(1, 40m), CancellationToken.None));

            await _repo.DidNotReceive().CargarCostoAFamiliarAsync(Arg.Any<int>(), Arg.Any<decimal>());
            await _repo.DidNotReceive().ActualizarPrescripcionAsync(Arg.Any<Prescripcion>());
        }

        [Fact] // FAR-03: un costo negativo debe rechazarse
        public async Task Costo_negativo_se_rechaza_y_no_guarda_nada()
        {
            var p = new Prescripcion { IdPrescripcion = 2, IdVisitaMedica = 10, Estado = "Pendiente" };
            _repo.ObtenerPrescripcionPorIdAsync(2).Returns(p);
            var handler = new DespacharPrescripcionCommandHandler(_repo);

            await Assert.ThrowsAsync<DatosInvalidosException>(() =>
                handler.Handle(new DespacharPrescripcionCommand(2, -5m), CancellationToken.None));

            await _repo.DidNotReceive().ActualizarPrescripcionAsync(Arg.Any<Prescripcion>());
            await _repo.DidNotReceive().CargarCostoAFamiliarAsync(Arg.Any<int>(), Arg.Any<decimal>());
        }

        [Fact] // FAR-02: el despacho normal cambia el estado y carga el costo una sola vez
        public async Task Despacho_valido_cambia_el_estado_y_carga_el_costo_una_vez()
        {
            var p = new Prescripcion { IdPrescripcion = 3, IdVisitaMedica = 10, Estado = "Pendiente" };
            _repo.ObtenerPrescripcionPorIdAsync(3).Returns(p);
            var handler = new DespacharPrescripcionCommandHandler(_repo);

            var exito = await handler.Handle(new DespacharPrescripcionCommand(3, 40m), CancellationToken.None);

            Assert.True(exito);
            Assert.Equal("Despachado", p.Estado);
            await _repo.Received(1).CargarCostoAFamiliarAsync(10, 40m);
        }

        [Fact] // FAR-05: un ID inexistente devuelve false (la API responde 404)
        public async Task Prescripcion_inexistente_devuelve_false()
        {
            _repo.ObtenerPrescripcionPorIdAsync(99999).Returns((Prescripcion?)null);
            var handler = new DespacharPrescripcionCommandHandler(_repo);

            var exito = await handler.Handle(new DespacharPrescripcionCommand(99999, 40m), CancellationToken.None);

            Assert.False(exito);
        }
    }
}
