using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Donaciones
{
    public record CrearDonacionCommand(string TipoDonante, string NombreDonante, decimal Monto, string Descripcion) : IRequest<int>;
}
