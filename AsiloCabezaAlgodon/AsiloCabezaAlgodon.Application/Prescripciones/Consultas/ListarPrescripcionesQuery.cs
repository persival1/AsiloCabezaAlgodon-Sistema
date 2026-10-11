using MediatR;

namespace AsiloCabezaAlgodon.Application.Prescripciones.Consultas
{
    public record PrescripcionListaDto(
        int IdPrescripcion,
        string NombreMedicamento,
        string Dosis,
        string Indicaciones,
        string Estado,
        decimal? Costo,
        string NombreInterno,
        DateTime FechaVisita
    );

    public record ListarPrescripcionesQuery() : IRequest<List<PrescripcionListaDto>>;
}
