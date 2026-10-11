using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Consultas
{
    public class ListarSolicitudesQueryHandler : IRequestHandler<ListarSolicitudesQuery, List<SolicitudDto>>
    {
        private readonly ISolicitudRepository _solicitudRepository;

        public ListarSolicitudesQueryHandler(ISolicitudRepository solicitudRepository)
        {
            _solicitudRepository = solicitudRepository;
        }

        public async Task<List<SolicitudDto>> Handle(ListarSolicitudesQuery request, CancellationToken cancellationToken)
        {
            var solicitudes = await _solicitudRepository.ObtenerTodasAsync();

            var filtradas = solicitudes.AsEnumerable();

            if (request.IdMedicoGeneral.HasValue)
                filtradas = filtradas.Where(s => s.IdMedicoGeneral == request.IdMedicoGeneral);

            if (request.IdEspecialista.HasValue)
                filtradas = filtradas.Where(s => s.IdEspecialista == request.IdEspecialista);

            if (request.IdEnfermero.HasValue)
                filtradas = filtradas.Where(s => s.IdEnfermero == request.IdEnfermero);

            return filtradas.Select(s => new SolicitudDto(
                s.IdSolicitud,
                s.Motivo,
                s.EspecialidadRequerida,
                s.FechaCreacion,
                s.Estado,
                s.IdInterno,
                $"{s.Interno.Nombres} {s.Interno.Apellidos}",
                s.IdMedicoGeneral,
                s.MedicoGeneral.NombreCompleto,
                s.IdEspecialista,
                s.Especialista?.NombreCompleto,
                s.Especialista?.Especialidad,
                s.IdEnfermero,
                s.Enfermero?.NombreCompleto,
                s.FechaHoraCita
            )).ToList();
        }
    }
}
