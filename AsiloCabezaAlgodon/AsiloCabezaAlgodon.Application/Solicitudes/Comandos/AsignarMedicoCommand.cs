using MediatR;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public record AsignarMedicoCommand(
        int IdSolicitud,
        int IdEspecialista,
        int? IdEnfermero,
        DateTime FechaHoraCita
    ) : IRequest<bool>;
}
