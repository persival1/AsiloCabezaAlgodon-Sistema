using MediatR;

namespace AsiloCabezaAlgodon.Application.Internos.Consultas
{
    public record InternoDto(
        int IdInterno,
        string Nombres,
        string Apellidos,
        DateTime FechaNacimiento,
        DateTime FechaIngreso,
        string MotivoIngreso,
        string Estado,
        int? IdFamiliar,
        string? NombreFamiliar,
        string? CorreoFamiliar
    );

    // AC-04: con IdEspecialista, solo los internos de las solicitudes asignadas a ese especialista
    public record ListarInternosQuery(int? IdEspecialista = null) : IRequest<List<InternoDto>>;
}
