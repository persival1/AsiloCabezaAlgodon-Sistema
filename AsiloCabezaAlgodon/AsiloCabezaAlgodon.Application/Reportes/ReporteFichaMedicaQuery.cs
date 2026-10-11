using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 2: análisis médicos por paciente (ficha médica completa: motivo de reclusión y medicamentos).
    public record VisitaResumenDto(DateTime FechaVisita, string Motivo, string Diagnostico, string NombreEspecialista, string? EspecialidadEspecialista, List<string> MedicamentosRecetados);

    public record FichaMedicaReporteDto(
        int IdInterno,
        string NombreCompleto,
        DateTime FechaIngreso,
        string MotivoIngreso,
        List<string> Psicopatologias,
        List<string> MedicamentosCajon,
        List<VisitaResumenDto> Visitas
    );

    public record ReporteFichaMedicaQuery(int IdInterno) : IRequest<FichaMedicaReporteDto?>;
}
