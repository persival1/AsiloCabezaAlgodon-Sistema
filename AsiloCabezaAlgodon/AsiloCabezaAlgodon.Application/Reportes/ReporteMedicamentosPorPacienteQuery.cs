using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 7: medicamentos aplicados por paciente.
    public record MedicamentoReporteDto(string NombreMedicamento, string Dosis, string Indicaciones, DateTime FechaVisita, string Estado, decimal? Costo);

    public record ReporteMedicamentosPorPacienteQuery(int IdInterno) : IRequest<List<MedicamentoReporteDto>>;
}
