using MediatR;
using AsiloCabezaAlgodon.Application.Comun.Interfaces;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    public class ReporteEntradasQueryHandler : IRequestHandler<ReporteEntradasQuery, ReporteEntradasDto>
    {
        private readonly ICajaRepository _cajaRepository;

        public ReporteEntradasQueryHandler(ICajaRepository cajaRepository)
        {
            _cajaRepository = cajaRepository;
        }

        public async Task<ReporteEntradasDto> Handle(ReporteEntradasQuery request, CancellationToken cancellationToken)
        {
            var donaciones = await _cajaRepository.ObtenerDonacionesAsync();
            var cobros = await _cajaRepository.ObtenerCobrosAsync();

            bool EnRango(DateTime fecha)
            {
                if (request.FechaInicio.HasValue && fecha < request.FechaInicio.Value.Date)
                    return false;

                if (request.FechaFin.HasValue && fecha > request.FechaFin.Value.Date.AddDays(1).AddTicks(-1))
                    return false;

                return true;
            }

            var donacionesFiltradas = donaciones.Where(d => EnRango(d.Fecha)).ToList();
            var cobrosFiltrados = cobros.Where(c => EnRango(c.Fecha)).ToList();

            var entradas = donacionesFiltradas
                .Select(d => new EntradaDto(d.Fecha, "Donación", d.NombreDonante, d.Monto))
                .Concat(cobrosFiltrados.Select(c => new EntradaDto(c.Fecha, "Cobro", $"{c.Familiar.Nombres} {c.Familiar.Apellidos}", c.Monto)))
                .OrderByDescending(e => e.Fecha)
                .ToList();

            var totalDonaciones = donacionesFiltradas.Sum(d => d.Monto);
            var totalCobros = cobrosFiltrados.Sum(c => c.Monto);

            return new ReporteEntradasDto(entradas, totalDonaciones, totalCobros, totalDonaciones + totalCobros);
        }
    }
}
