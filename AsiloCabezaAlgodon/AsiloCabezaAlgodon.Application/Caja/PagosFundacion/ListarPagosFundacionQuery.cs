using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Caja.PagosFundacion
{
    public record PagoFundacionDto(int IdPagoFundacion, DateTime Fecha, decimal Monto, string NumeroComprobante, string Descripcion);

    public record ListarPagosFundacionQuery() : IRequest<List<PagoFundacionDto>>;

    public class ListarPagosFundacionQueryHandler : IRequestHandler<ListarPagosFundacionQuery, List<PagoFundacionDto>>
    {
        private readonly ICajaRepository _cajaRepository;

        public ListarPagosFundacionQueryHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<List<PagoFundacionDto>> Handle(ListarPagosFundacionQuery request, CancellationToken cancellationToken)
        {
            var pagos = await _cajaRepository.ObtenerPagosFundacionAsync();
            return pagos.Select(p => new PagoFundacionDto(p.IdPagoFundacion, p.Fecha, p.Monto, p.NumeroComprobante, p.Descripcion)).ToList();
        }
    }
}
