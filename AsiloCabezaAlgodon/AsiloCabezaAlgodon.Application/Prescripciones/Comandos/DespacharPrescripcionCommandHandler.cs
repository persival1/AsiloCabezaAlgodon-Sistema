using MediatR;
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

            prescripcion.Costo = request.Costo;
            prescripcion.FechaDespacho = DateTime.Now;
            prescripcion.Estado = "Despachado";

            await _visitaMedicaRepository.ActualizarPrescripcionAsync(prescripcion);
            await _visitaMedicaRepository.CargarCostoAFamiliarAsync(prescripcion.IdVisitaMedica, request.Costo);
            return true;
        }
    }
}
