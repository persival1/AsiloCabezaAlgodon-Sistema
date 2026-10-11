using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteMedicamentosPorPacienteQueryHandler : IRequestHandler<ReporteMedicamentosPorPacienteQuery, List<MedicamentoReporteDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReporteMedicamentosPorPacienteQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<List<MedicamentoReporteDto>> Handle(ReporteMedicamentosPorPacienteQuery request, CancellationToken cancellationToken)
        {
            var prescripciones = await _visitaMedicaRepository.ObtenerPrescripcionesAsync();

            return prescripciones
                .Where(p => p.VisitaMedica.Solicitud.IdInterno == request.IdInterno)
                .Select(p => new MedicamentoReporteDto(p.NombreMedicamento, p.Dosis, p.Indicaciones, p.VisitaMedica.FechaVisita, p.Estado, p.Costo))
                .ToList();
        }
    }
}
