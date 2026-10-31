using MediatR;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public record AsignarMedicoCommand(
        int IdSolicitud,
        int IdEspecialista,
        int? IdEnfermero,
        DateTime FechaHoraCita
    ) : IRequest<ResultadoAsignacion?>;

    // null = la solicitud no existe (404). CorreoEnviado le permite a la API decir la verdad
    // sobre el correo al familiar (semana 3, caso COR-04).
    public record ResultadoAsignacion(bool CorreoEnviado);
}
