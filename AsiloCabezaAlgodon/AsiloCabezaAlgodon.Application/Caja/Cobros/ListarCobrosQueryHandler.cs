using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Caja.Cobros
{
    public class ListarCobrosQueryHandler : IRequestHandler<ListarCobrosQuery, List<CobroDto>>
    {
        private readonly ICajaRepository _cajaRepository;

        public ListarCobrosQueryHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<List<CobroDto>> Handle(ListarCobrosQuery request, CancellationToken cancellationToken)
        {
            var cobros = await _cajaRepository.ObtenerCobrosAsync();

            return cobros.Select(c => new CobroDto(
                c.IdCobro,
                c.Concepto,
                c.Monto,
                c.Fecha,
                $"{c.Familiar.Nombres} {c.Familiar.Apellidos}",
                $"{c.Familiar.Interno.Nombres} {c.Familiar.Interno.Apellidos}",
                c.Familiar.SaldoPendiente,
                c.IdVisitaMedica
            )).ToList();
        }
    }
}
