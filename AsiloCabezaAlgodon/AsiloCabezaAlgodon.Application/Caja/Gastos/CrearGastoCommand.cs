using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Gastos
{
    public record CrearGastoCommand(string Concepto, decimal Monto) : IRequest<int>;
}
