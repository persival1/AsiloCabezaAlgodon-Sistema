using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;
using AsiloCabezaAlgodon.Domain.Entidad;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public class CrearSolicitudCommandHandler : IRequestHandler<CrearSolicitudCommand, int>
    {
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IInternoRepository _internoRepository;

        public CrearSolicitudCommandHandler(ISolicitudRepository solicitudRepository, IInternoRepository internoRepository)
        {
            _solicitudRepository = solicitudRepository;
            _internoRepository = internoRepository;
        }

        public async Task<int> Handle(CrearSolicitudCommand request, CancellationToken cancellationToken)
        {
            // Revisión del 30/10/2026: antes, con un interno inexistente la base rechazaba el
            // registro y la API respondía 500; y se aceptaban solicitudes sin motivo.
            Validar.Texto(request.Motivo, "motivo");
            if (await _internoRepository.ObtenerPorIdAsync(request.IdInterno) == null)
                throw new DatosInvalidosException("El interno indicado no existe.");

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
