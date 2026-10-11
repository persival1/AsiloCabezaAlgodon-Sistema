using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteExamenesPorPacienteQueryHandler : IRequestHandler<ReporteExamenesPorPacienteQuery, List<ExamenReporteDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReporteExamenesPorPacienteQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<List<ExamenReporteDto>> Handle(ReporteExamenesPorPacienteQuery request, CancellationToken cancellationToken)
        {
            var examenes = await _visitaMedicaRepository.ObtenerExamenesAsync();

            return examenes
                .Where(e => e.VisitaMedica.Solicitud.IdInterno == request.IdInterno)
                .Select(e => new ExamenReporteDto(e.NombreExamen, e.VisitaMedica.FechaVisita, e.Estado, e.Resultado, e.Costo))
                .ToList();
        }
    }
}
