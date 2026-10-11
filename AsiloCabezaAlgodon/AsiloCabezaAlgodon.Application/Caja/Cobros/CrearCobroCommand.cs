using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Cobros
{
    public record CrearCobroCommand(int IdFamiliar, string Concepto, decimal Monto, int? IdVisitaMedica = null) : IRequest<int>;
}
