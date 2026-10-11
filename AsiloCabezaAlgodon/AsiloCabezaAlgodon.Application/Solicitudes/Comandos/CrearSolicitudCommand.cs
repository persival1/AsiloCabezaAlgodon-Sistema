using MediatR;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Comandos
{
    public record CrearSolicitudCommand(int IdInterno, string Motivo, string? EspecialidadRequerida, int IdMedicoGeneral) : IRequest<int>;
}
