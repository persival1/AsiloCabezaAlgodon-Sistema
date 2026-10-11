using MediatR;

namespace AsiloCabezaAlgodon.Application.Caja.Donaciones
{
    public record DonacionDto(int IdDonacion, string TipoDonante, string NombreDonante, decimal Monto, DateTime Fecha, string Descripcion);

    public record ListarDonacionesQuery() : IRequest<List<DonacionDto>>;
}
