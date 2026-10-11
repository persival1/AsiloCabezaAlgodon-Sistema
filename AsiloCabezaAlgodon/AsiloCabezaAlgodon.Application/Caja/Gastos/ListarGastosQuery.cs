using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Gastos
{
    public record GastoDto(int IdGasto, string Concepto, decimal Monto, DateTime Fecha);

    public record ListarGastosQuery() : IRequest<List<GastoDto>>;
}
