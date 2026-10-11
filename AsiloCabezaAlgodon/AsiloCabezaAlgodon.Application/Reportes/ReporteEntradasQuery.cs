using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 5: entradas de dinero — donaciones y cobros a familiares, en un rango de fechas opcional.
    public record EntradaDto(DateTime Fecha, string Tipo, string Origen, decimal Monto);

    public record ReporteEntradasDto(List<EntradaDto> Entradas, decimal TotalDonaciones, decimal TotalCobros, decimal TotalGeneral);

    public record ReporteEntradasQuery(DateTime? FechaInicio, DateTime? FechaFin) : IRequest<ReporteEntradasDto>;
}
