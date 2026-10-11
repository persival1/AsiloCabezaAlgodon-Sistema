using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public class AsignarMedicoCommandHandler : IRequestHandler<AsignarMedicoCommand, bool>
    {
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IEmailService _emailService;

        public AsignarMedicoCommandHandler(ISolicitudRepository solicitudRepository, IEmailService emailService)
        {
            _solicitudRepository = solicitudRepository;
            _emailService = emailService;
        }

        public async Task<bool> Handle(AsignarMedicoCommand request, CancellationToken cancellationToken)
        {
            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);

            if (solicitud == null)
                return false;

            solicitud.IdEspecialista = request.IdEspecialista;
            solicitud.IdEnfermero = request.IdEnfermero;
            solicitud.FechaHoraCita = request.FechaHoraCita;
            solicitud.Estado = "Asignada";

            await _solicitudRepository.ActualizarAsync(solicitud);

            var correoFamiliar = solicitud.Interno.Familiar?.Correo;
            if (!string.IsNullOrWhiteSpace(correoFamiliar))
            {
                var asunto = $"Cita médica asignada para {solicitud.Interno.Nombres} {solicitud.Interno.Apellidos}";
                var cuerpo =
                    $"Estimado/a familiar,\n\n" +
                    $"Se ha asignado una cita médica para {solicitud.Interno.Nombres} {solicitud.Interno.Apellidos}.\n" +
                    $"Motivo de la solicitud: {solicitud.Motivo}\n" +
                    $"Fecha y hora de la cita: {solicitud.FechaHoraCita:dd/MM/yyyy HH:mm}\n\n" +
                    $"Fundación Asilo Cabeza de Algodón.";

                await _emailService.EnviarAsync(correoFamiliar, asunto, cuerpo);
            }

            return true;
        }
    }
}
