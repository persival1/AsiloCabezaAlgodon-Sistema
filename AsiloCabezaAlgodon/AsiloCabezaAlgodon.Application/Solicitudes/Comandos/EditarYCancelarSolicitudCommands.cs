using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    // AC-08, CU-013: el médico general corrige el motivo o la especialidad de una solicitud
    // que él creó, mientras siga Pendiente (después ya hay una cita armada sobre esos datos).
    public record EditarSolicitudCommand(int IdSolicitud, string Motivo, string? EspecialidadRequerida, int IdUsuario) : IRequest<bool>;

    // AC-08, CU-014 y CU-019: cancelar.
    //   Médico general: solo sus solicitudes y solo mientras estén Pendientes.
    //   Administrador (la fundación): Pendientes o con cita Asignada.
    // Si ya había cita, se avisa al familiar por correo, igual que al asignarla.
    // La reprogramación (CU-018) usa PUT /Solicitudes/{id}/asignar, que acepta solicitudes Asignadas.
    public record CancelarSolicitudCommand(int IdSolicitud, int IdUsuario, string Rol) : IRequest<ResultadoAsignacion?>;

    public class EditarYCancelarSolicitudCommandHandler :
        IRequestHandler<EditarSolicitudCommand, bool>,
        IRequestHandler<CancelarSolicitudCommand, ResultadoAsignacion?>
    {
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IEmailService _emailService;

        public EditarYCancelarSolicitudCommandHandler(ISolicitudRepository solicitudRepository, IEmailService emailService)
        {
            _solicitudRepository = solicitudRepository;
            _emailService = emailService;
        }

        public async Task<bool> Handle(EditarSolicitudCommand request, CancellationToken cancellationToken)
        {
            Validar.Texto(request.Motivo, "motivo");
            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);
            if (solicitud == null)
                return false;
            if (solicitud.IdMedicoGeneral != request.IdUsuario)
                throw new AccesoDenegadoException("Solo el médico que creó la solicitud puede editarla.");
            if (solicitud.Estado != "Pendiente")
                throw new ConflictoException($"Solo se puede editar una solicitud Pendiente; esta está «{solicitud.Estado}».");

            solicitud.Motivo = request.Motivo.Trim();
            solicitud.EspecialidadRequerida = string.IsNullOrWhiteSpace(request.EspecialidadRequerida) ? null : request.EspecialidadRequerida.Trim();
            await _solicitudRepository.ActualizarAsync(solicitud);
            return true;
        }

        public async Task<ResultadoAsignacion?> Handle(CancelarSolicitudCommand request, CancellationToken cancellationToken)
        {
            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);
            if (solicitud == null)
                return null;

            if (request.Rol == "MedicoGeneral")
            {
                if (solicitud.IdMedicoGeneral != request.IdUsuario)
                    throw new AccesoDenegadoException("Solo el médico que creó la solicitud puede cancelarla.");
                if (solicitud.Estado != "Pendiente")
                    throw new ConflictoException("La solicitud ya tiene cita asignada: pedile a la fundación que la cancele.");
            }
            else if (solicitud.Estado != "Pendiente" && solicitud.Estado != "Asignada")
            {
                throw new ConflictoException($"No se puede cancelar una solicitud en estado «{solicitud.Estado}».");
            }

            var teniaCita = solicitud.Estado == "Asignada";
            solicitud.Estado = "Cancelada";
            await _solicitudRepository.ActualizarAsync(solicitud);

            var correoEnviado = false;
            var correo = solicitud.Interno?.Familiar?.Correo;
            if (teniaCita && !string.IsNullOrWhiteSpace(correo))
            {
                var nombre = $"{solicitud.Interno!.Nombres} {solicitud.Interno.Apellidos}";
                correoEnviado = await _emailService.EnviarAsync(correo,
                    $"Cita médica cancelada para {nombre}",
                    $"Estimado/a familiar,\n\n" +
                    $"La cita médica de {nombre} del {solicitud.FechaHoraCita:dd/MM/yyyy HH:mm} fue cancelada.\n" +
                    $"Motivo de la solicitud: {solicitud.Motivo}\n\n" +
                    $"Fundación Asilo Cabeza de Algodón.");
            }
            return new ResultadoAsignacion(correoEnviado);
        }
    }
}
