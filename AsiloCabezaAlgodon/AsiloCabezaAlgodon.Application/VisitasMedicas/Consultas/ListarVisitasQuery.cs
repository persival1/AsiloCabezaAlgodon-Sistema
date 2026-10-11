using MediatR;

namespace AsiloCabezaAlgodon.Application.VisitasMedicas.Consultas
{
    public record ExamenDto(int IdExamenSolicitado, string NombreExamen, string Estado, string? Resultado, DateTime? FechaResultado, decimal? Costo);

    public record PrescripcionDto(int IdPrescripcion, string NombreMedicamento, string Dosis, string Indicaciones, string Estado, decimal? Costo);

    public record VisitaDto(
        int IdVisitaMedica,
        int IdSolicitud,
        int IdInterno,
        string NombreInterno,
        string NombreEspecialista,
        string? EspecialidadEspecialista,
        DateTime FechaVisita,
        string Motivo,
        string Diagnostico,
        string Observaciones,
        decimal CostoConsulta,
        List<ExamenDto> Examenes,
        List<PrescripcionDto> Prescripciones,
        decimal TotalCargos,
        decimal TotalCobrado,
        string EstadoPago
    );

    // idEspecialista: si se envía, solo trae las visitas realizadas por ese especialista.
    // idInterno: si se envía, solo trae las visitas de ese interno (para ver su ficha médica completa).
    public record ListarVisitasQuery(int? IdEspecialista = null, int? IdInterno = null) : IRequest<List<VisitaDto>>;
}
