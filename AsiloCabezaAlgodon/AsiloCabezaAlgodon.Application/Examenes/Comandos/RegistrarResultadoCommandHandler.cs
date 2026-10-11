using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Examenes.Comandos
{
    public class RegistrarResultadoCommandHandler : IRequestHandler<RegistrarResultadoCommand, bool>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public RegistrarResultadoCommandHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<bool> Handle(RegistrarResultadoCommand request, CancellationToken cancellationToken)
        {
            var examen = await _visitaMedicaRepository.ObtenerExamenPorIdAsync(request.IdExamenSolicitado);

            if (examen == null)
                return false;

            examen.Resultado = request.Resultado;
            examen.FechaResultado = DateTime.Now;
            examen.Estado = "ResultadoRegistrado";
            examen.Costo = request.Costo;

            await _visitaMedicaRepository.ActualizarExamenAsync(examen);
            await _visitaMedicaRepository.CargarCostoAFamiliarAsync(examen.IdVisitaMedica, request.Costo);
            return true;
        }
    }
}
