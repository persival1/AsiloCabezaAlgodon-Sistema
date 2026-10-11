using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteCostosPorCitaQueryHandler : IRequestHandler<ReporteCostosPorCitaQuery, List<CostoCitaDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReporteCostosPorCitaQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<List<CostoCitaDto>> Handle(ReporteCostosPorCitaQuery request, CancellationToken cancellationToken)
        {
            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();

            return visitas.Select(v =>
            {
                var costoExamenes = v.ExamenesSolicitados.Sum(e => e.Costo ?? 0);
                var costoMedicamentos = v.Prescripciones.Sum(p => p.Costo ?? 0);

                return new CostoCitaDto(
                    v.IdVisitaMedica,
                    $"{v.Solicitud.Interno.Nombres} {v.Solicitud.Interno.Apellidos}",
                    v.FechaVisita,
                    v.CostoConsulta,
                    costoExamenes,
                    costoMedicamentos,
                    v.CostoConsulta + costoExamenes + costoMedicamentos
                );
            }).ToList();
        }
    }
}
