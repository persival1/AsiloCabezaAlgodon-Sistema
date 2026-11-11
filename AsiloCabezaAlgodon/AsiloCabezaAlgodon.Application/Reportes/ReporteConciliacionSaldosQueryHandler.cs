using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteConciliacionSaldosQueryHandler : IRequestHandler<ReporteConciliacionSaldosQuery, List<SaldoConciliacionDto>>
    {
        private readonly IInternoRepository _internoRepository;
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly ICajaRepository _cajaRepository;

        public ReporteConciliacionSaldosQueryHandler(
            IInternoRepository internoRepository,
            IVisitaMedicaRepository visitaMedicaRepository,
            ICajaRepository cajaRepository)
        {
            _internoRepository = internoRepository;
            _visitaMedicaRepository = visitaMedicaRepository;
            _cajaRepository = cajaRepository;
        }

        public async Task<List<SaldoConciliacionDto>> Handle(ReporteConciliacionSaldosQuery request, CancellationToken cancellationToken)
        {
            var internos = await _internoRepository.ObtenerTodosAsync();
            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();
            var cobros = await _cajaRepository.ObtenerCobrosAsync();

            var resultado = new List<SaldoConciliacionDto>();

            foreach (var interno in internos.Where(i => i.Familiar != null))
            {
                var familiar = interno.Familiar!;
                var visitasDelInterno = visitas.Where(v => v.Solicitud.IdInterno == interno.IdInterno).ToList();

                // Se suman solo los montos mayores que 0, igual que CargarCostoAFamiliarAsync,
                // que es el método que carga los costos al familiar cuando ocurre cada movimiento.
                var cargos =
                    visitasDelInterno.Where(v => v.CostoConsulta > 0).Sum(v => v.CostoConsulta) +
                    visitasDelInterno.SelectMany(v => v.ExamenesSolicitados).Where(e => e.Costo > 0).Sum(e => e.Costo!.Value) +
                    visitasDelInterno.SelectMany(v => v.Prescripciones).Where(p => p.Costo > 0).Sum(p => p.Costo!.Value);

                // AC-01: solo cuentan los pagos ligados a una visita (la cuota mensual no paga cargos médicos)
                var totalCobros = cobros.Where(c => c.IdFamiliar == familiar.IdFamiliar && c.IdVisitaMedica != null).Sum(c => c.Monto);
                var recalculado = cargos - totalCobros;
                var diferencia = familiar.SaldoPendiente - recalculado;

                resultado.Add(new SaldoConciliacionDto(
                    familiar.IdFamiliar,
                    $"{familiar.Nombres} {familiar.Apellidos}",
                    $"{interno.Nombres} {interno.Apellidos}",
                    cargos,
                    totalCobros,
                    recalculado,
                    familiar.SaldoPendiente,
                    diferencia,
                    diferencia == 0));
            }

            return resultado;
        }
    }
}
