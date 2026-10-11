using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Caja.Gastos
{
    public class ListarGastosQueryHandler : IRequestHandler<ListarGastosQuery, List<GastoDto>>
    {
        private readonly ICajaRepository _cajaRepository;

        public ListarGastosQueryHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<List<GastoDto>> Handle(ListarGastosQuery request, CancellationToken cancellationToken)
        {
            var gastos = await _cajaRepository.ObtenerGastosAsync();

            return gastos.Select(g => new GastoDto(g.IdGasto, g.Concepto, g.Monto, g.Fecha)).ToList();
        }
    }
}
