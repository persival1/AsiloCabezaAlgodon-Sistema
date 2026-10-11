using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Consultas
{
    public class ListarVisitasQueryHandler : IRequestHandler<ListarVisitasQuery, List<VisitaDto>>
    {
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;
        private readonly ICajaRepository _cajaRepository;

        public ListarVisitasQueryHandler(IVisitaMedicaRepository visitaMedicaRepository, ICajaRepository cajaRepository)
        {
            _visitaMedicaRepository = visitaMedicaRepository;
            _cajaRepository = cajaRepository;
        }

        public async Task<List<VisitaDto>> Handle(ListarVisitasQuery request, CancellationToken cancellationToken)
        {
            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();
            var cobros = await _cajaRepository.ObtenerCobrosAsync();

            var filtradas = visitas.AsEnumerable();

            if (request.IdEspecialista.HasValue)
                filtradas = filtradas.Where(v => v.Solicitud.IdEspecialista == request.IdEspecialista);

            if (request.IdInterno.HasValue)
                filtradas = filtradas.Where(v => v.Solicitud.IdInterno == request.IdInterno);

            return filtradas.Select(v =>
            {
                var totalCargos = v.CostoConsulta + v.ExamenesSolicitados.Sum(e => e.Costo ?? 0) + v.Prescripciones.Sum(p => p.Costo ?? 0);
                var totalCobrado = cobros.Where(c => c.IdVisitaMedica == v.IdVisitaMedica).Sum(c => c.Monto);
                var estadoPago = totalCargos <= 0 ? "SinCargos" : (totalCobrado >= totalCargos ? "Pagada" : "PendientePago");

                return new VisitaDto(
                    v.IdVisitaMedica,
                    v.IdSolicitud,
                    v.Solicitud.IdInterno,
                    $"{v.Solicitud.Interno.Nombres} {v.Solicitud.Interno.Apellidos}",
                    v.Solicitud.Especialista?.NombreCompleto ?? "—",
                    v.Solicitud.Especialista?.Especialidad,
                    v.FechaVisita,
                    v.Solicitud.Motivo,
                    v.Diagnostico,
                    v.Observaciones,
                    v.CostoConsulta,
                    v.ExamenesSolicitados.Select(e => new ExamenDto(e.IdExamenSolicitado, e.NombreExamen, e.Estado, e.Resultado, e.FechaResultado, e.Costo)).ToList(),
                    v.Prescripciones.Select(p => new PrescripcionDto(p.IdPrescripcion, p.NombreMedicamento, p.Dosis, p.Indicaciones, p.Estado, p.Costo)).ToList(),
                    totalCargos,
                    totalCobrado,
                    estadoPago
                );
            }).ToList();
        }
    }
}
