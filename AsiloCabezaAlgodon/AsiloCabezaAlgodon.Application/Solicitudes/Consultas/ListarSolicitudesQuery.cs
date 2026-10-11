using MediatR;

namespace AsiloCabezaAlgodon.Application.Solicitudes.Consultas
{
    public record SolicitudDto(
        int IdSolicitud,
        string Motivo,
        string? EspecialidadRequerida,
        DateTime FechaCreacion,
        string Estado,
        int IdInterno,
        string NombreInterno,
        int IdMedicoGeneral,
        string NombreMedicoGeneral,
        int? IdEspecialista,
        string? NombreEspecialista,
        string? EspecialidadEspecialista,
        int? IdEnfermero,
        string? NombreEnfermero,
        DateTime? FechaHoraCita
    );

    // idMedicoGeneral: si se envía, solo trae las solicitudes creadas por ese médico general.
    // idEspecialista: si se envía, solo trae las asignadas a ese especialista.
    public record ListarSolicitudesQuery(int? IdMedicoGeneral = null, int? IdEspecialista = null, int? IdEnfermero = null) : IRequest<List<SolicitudDto>>;
}
