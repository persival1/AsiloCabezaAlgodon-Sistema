using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 3: cobros por paciente en un rango de fechas, incluyendo los gastos médicos (cargos) como detalle.
    public record CobroDetalleDto(DateTime Fecha, string Concepto, decimal Monto);

    public record CargoMedicoDetalleDto(DateTime Fecha, string Concepto, decimal Monto);

    public record ReporteCobrosPorPacienteDto(
        string NombreInterno,
        decimal TotalCobrado,
        decimal TotalCargosMedicos,
        List<CobroDetalleDto> Cobros,
        List<CargoMedicoDetalleDto> CargosMedicos
    );

    public record ReporteCobrosPorPacienteQuery(int IdInterno, DateTime FechaInicio, DateTime FechaFin) : IRequest<ReporteCobrosPorPacienteDto?>;
}
