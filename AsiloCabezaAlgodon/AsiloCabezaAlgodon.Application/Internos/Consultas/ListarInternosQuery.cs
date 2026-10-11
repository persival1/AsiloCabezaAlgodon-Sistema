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

    public record ListarInternosQuery() : IRequest<List<InternoDto>>;
}
