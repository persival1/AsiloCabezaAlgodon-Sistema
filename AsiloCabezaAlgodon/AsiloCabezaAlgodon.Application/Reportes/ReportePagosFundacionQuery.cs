using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 4: pagos a la fundación (consultas + laboratorio + farmacia), en un rango de fechas opcional.
    public record PagosFundacionDto(decimal TotalConsultas, decimal TotalLaboratorio, decimal TotalFarmacia, decimal TotalGeneral);

    public record ReportePagosFundacionQuery(DateTime? FechaInicio, DateTime? FechaFin) : IRequest<PagosFundacionDto>;
}
