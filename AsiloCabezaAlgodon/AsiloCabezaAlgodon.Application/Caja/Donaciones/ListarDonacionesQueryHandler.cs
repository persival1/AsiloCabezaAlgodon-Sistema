using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Caja.Donaciones
{
    public class ListarDonacionesQueryHandler : IRequestHandler<ListarDonacionesQuery, List<DonacionDto>>
    {
        private readonly ICajaRepository _cajaRepository;

        public ListarDonacionesQueryHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<List<DonacionDto>> Handle(ListarDonacionesQuery request, CancellationToken cancellationToken)
        {
            var donaciones = await _cajaRepository.ObtenerDonacionesAsync();

            return donaciones.Select(d => new DonacionDto(d.IdDonacion, d.TipoDonante, d.NombreDonante, d.Monto, d.Fecha, d.Descripcion)).ToList();
        }
    }
}
