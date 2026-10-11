using MediatR;

namespace AsiloCabezaAlgodon.Application.Reportes
{
    // Reporte 1: costos de cada cita por paciente, incluyendo exámenes de laboratorio y medicamentos.
    public record CostoCitaDto(
        int IdVisitaMedica,
        string NombreInterno,
        DateTime FechaVisita,
        decimal CostoConsulta,
        decimal CostoExamenes,
        decimal CostoMedicamentos,
        decimal CostoTotal
    );

    public record ReporteCostosPorCitaQuery() : IRequest<List<CostoCitaDto>>;
}
