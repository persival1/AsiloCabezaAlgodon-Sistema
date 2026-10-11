using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public class CrearSolicitudCommandHandler : IRequestHandler<CrearSolicitudCommand, int>
    {
        private readonly ISolicitudRepository _solicitudRepository;

        public CrearSolicitudCommandHandler(ISolicitudRepository solicitudRepository)
        {
            _solicitudRepository = solicitudRepository;
        }

        public async Task<int> Handle(CrearSolicitudCommand request, CancellationToken cancellationToken)
        {
            var solicitud = new Solicitud
            {
                IdInterno = request.IdInterno,
                Motivo = request.Motivo,
                EspecialidadRequerida = request.EspecialidadRequerida,
                IdMedicoGeneral = request.IdMedicoGeneral,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            var creada = await _solicitudRepository.CrearAsync(solicitud);
            return creada.IdSolicitud;
        }
    }
}
