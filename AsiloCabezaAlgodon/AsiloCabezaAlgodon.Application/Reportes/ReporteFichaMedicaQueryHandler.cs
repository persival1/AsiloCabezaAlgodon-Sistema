using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteFichaMedicaQueryHandler : IRequestHandler<ReporteFichaMedicaQuery, FichaMedicaReporteDto?>
    {
        private readonly IInternoRepository _internoRepository;
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReporteFichaMedicaQueryHandler(IInternoRepository internoRepository, IVisitaMedicaRepository visitaMedicaRepository)
        {
            _internoRepository = internoRepository;
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<FichaMedicaReporteDto?> Handle(ReporteFichaMedicaQuery request, CancellationToken cancellationToken)
        {
            var interno = await _internoRepository.ObtenerPorIdAsync(request.IdInterno);
            if (interno == null)
                return null;

            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();
            var visitasDelInterno = visitas
                .Where(v => v.Solicitud.IdInterno == request.IdInterno)
                .Select(v => new VisitaResumenDto(
                    v.FechaVisita,
                    v.Solicitud.Motivo,
                    v.Diagnostico,
                    v.Solicitud.Especialista?.NombreCompleto ?? "—",
                    v.Solicitud.Especialista?.Especialidad,
                    v.Prescripciones.Select(p => $"{p.NombreMedicamento} ({p.Dosis})").ToList()
                ))
                .ToList();

            return new FichaMedicaReporteDto(
                interno.IdInterno,
                $"{interno.Nombres} {interno.Apellidos}",
                interno.FechaIngreso,
                interno.MotivoIngreso,
                interno.Psicopatologias.Select(p => p.Nombre).ToList(),
                interno.MedicamentosCajon.Select(m => $"{m.Nombre} ({m.Dosis}, {m.Frecuencia})").ToList(),
                visitasDelInterno
            );
        }
    }
}
