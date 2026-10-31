using MediatR;
using AsiloCabezaAlgodon.Application.Comun;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public class AsignarMedicoCommandHandler : IRequestHandler<AsignarMedicoCommand, ResultadoAsignacion?>
    {
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;

        public AsignarMedicoCommandHandler(ISolicitudRepository solicitudRepository, IUsuarioRepository usuarioRepository, IEmailService emailService)
        {
            _solicitudRepository = solicitudRepository;
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
        }

        public async Task<ResultadoAsignacion?> Handle(AsignarMedicoCommand request, CancellationToken cancellationToken)
        {
            var solicitud = await _solicitudRepository.ObtenerPorIdAsync(request.IdSolicitud);

            if (solicitud == null)
                return null;

            // CORRECCIÓN (plan de pruebas SQA, semana 3). Antes no se revisaba nada:
            // EXP-07: una solicitud Atendida (o Cancelada) ya no se puede asignar de nuevo.
            if (solicitud.Estado != "Pendiente" && solicitud.Estado != "Asignada")
                throw new ConflictoException($"No se puede asignar una solicitud en estado «{solicitud.Estado}».");

            // EXP-06: la cita no puede quedar en el pasado, y cada persona debe tener el rol correcto.
            if (request.FechaHoraCita < DateTime.Now)
                throw new DatosInvalidosException("La fecha de la cita no puede estar en el pasado.");
            if (!await _usuarioRepository.TieneRolAsync(request.IdEspecialista, "Especialista"))
                throw new DatosInvalidosException("El usuario elegido como especialista no tiene el rol Especialista.");
            if (request.IdEnfermero.HasValue && !await _usuarioRepository.TieneRolAsync(request.IdEnfermero.Value, "Enfermero"))
                throw new DatosInvalidosException("El usuario elegido como enfermero(a) no tiene el rol Enfermero.");

            solicitud.IdEspecialista = request.IdEspecialista;
            solicitud.IdEnfermero = request.IdEnfermero;
            solicitud.FechaHoraCita = request.FechaHoraCita;
            solicitud.Estado = "Asignada";

            await _solicitudRepository.ActualizarAsync(solicitud);

            // COR-02 a COR-04: si el correo no sale, la asignación se conserva igual, pero se
            // informa que el correo no se envió en lugar de decir que sí.
            var correoEnviado = false;
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

                correoEnviado = await _emailService.EnviarAsync(correoFamiliar, asunto, cuerpo);
            }

            return new ResultadoAsignacion(correoEnviado);
        }
    }
}
