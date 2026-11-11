using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReportePagosFundacionQueryHandler : IRequestHandler<ReportePagosFundacionQuery, PagosFundacionDto>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly ICajaRepository _cajaRepository;

        public ReportePagosFundacionQueryHandler(IVisitaMedicaRepository visitaMedicaRepository, ICajaRepository cajaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
            _cajaRepository = cajaRepository;
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

            // AC-07: pagos del rango y saldo acumulado con la fundación
            var todosLosPagos = await _cajaRepository.ObtenerPagosFundacionAsync();
            var pagosDelRango = todosLosPagos.Where(p =>
                (!request.FechaInicio.HasValue || p.Fecha >= request.FechaInicio.Value.Date) &&
                (!request.FechaFin.HasValue || p.Fecha <= request.FechaFin.Value.Date.AddDays(1).AddTicks(-1))).ToList();
            var adeudado = await _cajaRepository.TotalCargosMedicosAsync();
            var pagado = todosLosPagos.Sum(p => p.Monto);

            return new PagosFundacionDto(
                totalConsultas, totalLaboratorio, totalFarmacia, totalConsultas + totalLaboratorio + totalFarmacia,
                pagosDelRango.Sum(p => p.Monto), adeudado, pagado, adeudado - pagado,
                pagosDelRango.Select(p => new AsiloCabezaAlgodon.Application.Caja.PagosFundacion.PagoFundacionDto(
                    p.IdPagoFundacion, p.Fecha, p.Monto, p.NumeroComprobante, p.Descripcion)).ToList());
        }
    }
}
