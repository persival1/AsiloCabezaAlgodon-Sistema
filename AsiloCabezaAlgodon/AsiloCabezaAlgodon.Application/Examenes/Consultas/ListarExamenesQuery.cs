using MediatR;

namespace AsiloCabezaAlgodon.Application.Examenes.Consultas
{
    public record ExamenListaDto(
        int IdExamenSolicitado,
        string NombreExamen,
        string Estado,
        string? Resultado,
        DateTime? FechaResultado,
        string NombreInterno,
        DateTime FechaVisita
    );

    public record ListarExamenesQuery() : IRequest<List<ExamenListaDto>>;
}
