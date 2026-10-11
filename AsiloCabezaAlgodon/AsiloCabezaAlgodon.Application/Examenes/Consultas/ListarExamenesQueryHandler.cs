using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Examenes.Consultas
{
    public class ListarExamenesQueryHandler : IRequestHandler<ListarExamenesQuery, List<ExamenListaDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ListarExamenesQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<List<ExamenListaDto>> Handle(ListarExamenesQuery request, CancellationToken cancellationToken)
        {
            var examenes = await _visitaMedicaRepository.ObtenerExamenesAsync();

            return examenes.Select(e => new ExamenListaDto(
                e.IdExamenSolicitado,
                e.NombreExamen,
                e.Estado,
                e.Resultado,
                e.FechaResultado,
                $"{e.VisitaMedica.Solicitud.Interno.Nombres} {e.VisitaMedica.Solicitud.Interno.Apellidos}",
                e.VisitaMedica.FechaVisita
            )).ToList();
        }
    }
}
