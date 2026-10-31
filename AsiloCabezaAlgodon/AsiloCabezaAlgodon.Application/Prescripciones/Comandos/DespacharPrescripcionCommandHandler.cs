using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Prescripciones.Comandos
{
    public class DespacharPrescripcionCommandHandler : IRequestHandler<DespacharPrescripcionCommand, bool>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public DespacharPrescripcionCommandHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<bool> Handle(DespacharPrescripcionCommand request, CancellationToken cancellationToken)
        {
            var prescripcion = await _visitaMedicaRepository.ObtenerPrescripcionPorIdAsync(request.IdPrescripcion);

            if (prescripcion == null)
                return false;

            // CORRECCIÓN (plan de pruebas SQA, caso FAR-05): antes se podía despachar dos veces
            // la misma prescripción y el costo se cargaba dos veces al familiar. Ahora solo se
            // despacha si sigue Pendiente; si no, se rechaza con 409 y no se toca el saldo.
            if (prescripcion.Estado != "Pendiente")
                throw new ConflictoException("Esta prescripción ya fue despachada.");

            // Caso FAR-03: costo negativo, con más de dos decimales o demasiado grande.
            // Se acepta Q0.00 (medicamento sin costo), que no se carga al familiar.
            Validar.Monto(request.Costo, "El costo", permiteCero: true);

            prescripcion.Costo = request.Costo;
            prescripcion.FechaDespacho = DateTime.Now;
            prescripcion.Estado = "Despachado";

            await _visitaMedicaRepository.ActualizarPrescripcionAsync(prescripcion);
            await _visitaMedicaRepository.CargarCostoAFamiliarAsync(prescripcion.IdVisitaMedica, request.Costo);
            return true;
        }
    }
}
