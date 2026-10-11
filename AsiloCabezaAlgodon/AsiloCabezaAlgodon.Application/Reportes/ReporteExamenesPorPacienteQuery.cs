using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 6: exámenes médicos realizados por paciente.
    public record ExamenReporteDto(string NombreExamen, DateTime FechaVisita, string Estado, string? Resultado, decimal? Costo);

    public record ReporteExamenesPorPacienteQuery(int IdInterno) : IRequest<List<ExamenReporteDto>>;
}
