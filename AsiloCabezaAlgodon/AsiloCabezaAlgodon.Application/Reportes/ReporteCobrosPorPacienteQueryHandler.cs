using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteCobrosPorPacienteQueryHandler : IRequestHandler<ReporteCobrosPorPacienteQuery, ReporteCobrosPorPacienteDto?>
    {
        private readonly IInternoRepository _internoRepository;
        private readonly ICajaRepository _cajaRepository;
        private readonly IVisitaMedicaRepository _visitaMedicaRepository;

        public ReporteCobrosPorPacienteQueryHandler(IInternoRepository internoRepository, ICajaRepository cajaRepository, IVisitaMedicaRepository visitaMedicaRepository)
        {
            _internoRepository = internoRepository;
            _cajaRepository = cajaRepository;
            _visitaMedicaRepository = visitaMedicaRepository;
        }

        public async Task<ReporteCobrosPorPacienteDto?> Handle(ReporteCobrosPorPacienteQuery request, CancellationToken cancellationToken)
        {
            var interno = await _internoRepository.ObtenerPorIdAsync(request.IdInterno);
            if (interno == null)
                return null;

            var fin = request.FechaFin.Date.AddDays(1).AddTicks(-1);

            var cobros = await _cajaRepository.ObtenerCobrosAsync();
            var cobrosDelInterno = cobros
                .Where(c => c.Familiar.IdInterno == request.IdInterno && c.Fecha >= request.FechaInicio.Date && c.Fecha <= fin)
                .Select(c => new CobroDetalleDto(c.Fecha, c.Concepto, c.Monto))
                .ToList();

            var visitas = await _visitaMedicaRepository.ObtenerTodasAsync();
            var visitasDelInterno = visitas.Where(v =>
                v.Solicitud.IdInterno == request.IdInterno && v.FechaVisita >= request.FechaInicio.Date && v.FechaVisita <= fin);

            var cargos = new List<CargoMedicoDetalleDto>();
            foreach (var visita in visitasDelInterno)
            {
                if (visita.CostoConsulta > 0)
                    cargos.Add(new CargoMedicoDetalleDto(visita.FechaVisita, "Consulta médica", visita.CostoConsulta));

                foreach (var examen in visita.ExamenesSolicitados.Where(e => e.Costo is > 0))
                    cargos.Add(new CargoMedicoDetalleDto(examen.FechaResultado ?? visita.FechaVisita, $"Examen: {examen.NombreExamen}", examen.Costo!.Value));

                foreach (var prescripcion in visita.Prescripciones.Where(p => p.Costo is > 0))
                    cargos.Add(new CargoMedicoDetalleDto(prescripcion.FechaDespacho ?? visita.FechaVisita, $"Medicamento: {prescripcion.NombreMedicamento}", prescripcion.Costo!.Value));
            }

            return new ReporteCobrosPorPacienteDto(
                $"{interno.Nombres} {interno.Apellidos}",
                cobrosDelInterno.Sum(c => c.Monto),
                cargos.Sum(c => c.Monto),
                cobrosDelInterno.OrderByDescending(c => c.Fecha).ToList(),
                cargos.OrderByDescending(c => c.Fecha).ToList()
            );
        }
    }
}
