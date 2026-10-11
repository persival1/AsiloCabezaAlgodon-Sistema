using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReportePagosFundacionQueryHandler : IRequestHandler<ReportePagosFundacionQuery, PagosFundacionDto>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReportePagosFundacionQueryHandler(IVisitaMedicaRepository visitaMedicaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<PagosFundacionDto> Handle(ReportePagosFundacionQuery request, CancellationToken cancellationToken)
        {
            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();

            if (request.FechaInicio.HasValue)
                visitas = visitas.Where(v => v.FechaVisita >= request.FechaInicio.Value.Date).ToList();

            if (request.FechaFin.HasValue)
            {
                var fin = request.FechaFin.Value.Date.AddDays(1).AddTicks(-1);
                visitas = visitas.Where(v => v.FechaVisita <= fin).ToList();
            }

            var totalConsultas = visitas.Sum(v => v.CostoConsulta);
            var totalLaboratorio = visitas.Sum(v => v.ExamenesSolicitados.Sum(e => e.Costo ?? 0));
            var totalFarmacia = visitas.Sum(v => v.Prescripciones.Sum(p => p.Costo ?? 0));

            return new PagosFundacionDto(totalConsultas, totalLaboratorio, totalFarmacia, totalConsultas + totalLaboratorio + totalFarmacia);
        }
    }
}
