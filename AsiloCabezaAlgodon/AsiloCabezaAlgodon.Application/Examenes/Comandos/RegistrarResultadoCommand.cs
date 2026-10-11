using MediatR;

namespace AsiloCabezaAlgodon.Application.Examenes.Comandos
{
    public record RegistrarResultadoCommand(int IdExamenSolicitado, string Resultado, decimal Costo) : IRequest<bool>;
}
