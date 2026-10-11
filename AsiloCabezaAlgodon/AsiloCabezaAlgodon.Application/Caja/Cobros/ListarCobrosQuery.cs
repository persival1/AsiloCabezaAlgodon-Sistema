using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Cobros
{
    public record CobroDto(int IdCobro, string Concepto, decimal Monto, DateTime Fecha, string NombreFamiliar, string NombreInterno, decimal SaldoPendienteFamiliar, int? IdVisitaMedica);

    public record ListarCobrosQuery() : IRequest<List<CobroDto>>;
}
