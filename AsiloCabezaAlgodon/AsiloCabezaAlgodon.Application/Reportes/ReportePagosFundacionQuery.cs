using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 4: pagos a la fundación. AC-07: además de lo que se le debe por consultas,
    // laboratorio y farmacia, muestra lo pagado y el saldo pendiente con la fundación.
    //   TotalGeneral  = cargos médicos del rango (lo que se le debe por ese periodo)
    //   TotalPagado   = pagos registrados en el rango
    //   AdeudadoAcumulado, PagadoAcumulado y SaldoPendiente = desde el inicio hasta hoy
    public record PagosFundacionDto(
        decimal TotalConsultas, decimal TotalLaboratorio, decimal TotalFarmacia, decimal TotalGeneral,
        decimal TotalPagado, decimal AdeudadoAcumulado, decimal PagadoAcumulado, decimal SaldoPendiente,
        List<AsiloCabezaAlgodon.Application.Caja.PagosFundacion.PagoFundacionDto> Pagos);

    public record ReportePagosFundacionQuery(DateTime? FechaInicio, DateTime? FechaFin) : IRequest<PagosFundacionDto>;
}
